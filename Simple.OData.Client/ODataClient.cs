using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace Simple.OData.Client
{
    internal class EntryMembers
    {
        private IDictionary<string, object> _properties = new Dictionary<string, object>();
        private List<KeyValuePair<string, object>> _associationsByValue = new List<KeyValuePair<string, object>>();
        private List<KeyValuePair<string, int>> _associationsByContentId = new List<KeyValuePair<string, int>>();

        public IDictionary<string, object> Properties { get { return _properties; } }
        public List<KeyValuePair<string, object>> AssociationsByValue { get { return _associationsByValue; } }
        public List<KeyValuePair<string, int>> AssociationsByContentId { get { return _associationsByContentId; } }

        public void AddProperty(string propertyName, object propertyValue)
        {
            _properties.Add(propertyName, propertyValue);
        }

        public void AddAssociationByValue(string associationName, object associatedData)
        {
            _associationsByValue.Add(new KeyValuePair<string, object>(associationName, associatedData));
        }

        public void AddAssociationByContentId(string associationName, int contentId)
        {
            _associationsByContentId.Add(new KeyValuePair<string, int>(associationName, contentId));
        }
    }

    public class ODataClient
    {
        private string _urlBase;
        private ISchema _schema;
        private RequestBuilder _requestBuilder;
        private RequestRunner _requestRunner;

        public ODataClient(string urlBase)
        {
            _urlBase = urlBase;
            _schema = Client.Schema.Get(urlBase);

            _requestBuilder = new CommandRequestBuilder(_urlBase);
            _requestRunner = new CommandRequestRunner();
        }

        public ODataClient(ODataBatch batch)
        {
            _urlBase = batch.RequestBuilder.UrlBase;
            _schema = Client.Schema.Get(_urlBase);

            _requestBuilder = batch.RequestBuilder;
            _requestRunner = batch.RequestRunner;
        }

        public ISchema Schema
        {
            get { return _schema; }
        }

        public string SchemaAsString
        {
            get { return SchemaProvider.FromUrl(_urlBase).SchemaAsString; }
        }

        public static ISchema GetSchema(string urlBase)
        {
            return Client.Schema.Get(urlBase);
        }

        public static string GetSchemaAsString(string urlBase)
        {
            return SchemaProvider.FromUrl(urlBase).SchemaAsString;
        }

        public static ISchema ParseSchemaString(string schemaString)
        {
            return SchemaProvider.FromMetadata(schemaString).Schema;
        }

        public static void SetPluralizer(IPluralizer pluralizer)
        {
            StringExtensions.SetPluralizer(pluralizer);
        }

        public IClientWithCommand From(string collectionName)
        {
            return new ODataClientWithCommand(this, _schema).From(collectionName);
        }

        public IEnumerable<IDictionary<string, object>> FindEntries(string commandText)
        {
            int totalCount;
            return FindEntries(commandText, false, false, out totalCount);
        }

        public IEnumerable<IDictionary<string, object>> FindEntries(string commandText, bool scalarResult)
        {
            int totalCount;
            return FindEntries(commandText, scalarResult, false, out totalCount);
        }

        public IEnumerable<IDictionary<string, object>> FindEntries(string commandText, out int totalCount)
        {
            return FindEntries(commandText, false, true, out totalCount);
        }

        public IEnumerable<IDictionary<string, object>> FindEntries(string commandText, bool scalarResult, out int totalCount)
        {
            return FindEntries(commandText, scalarResult, true, out totalCount);
        }

        private IEnumerable<IDictionary<string, object>> FindEntries(string commandText, bool scalarResult, bool setTotalCount, out int totalCount)
        {
            var command = HttpCommand.Get(commandText);
            _requestBuilder.AddCommandToRequest(command);
            return _requestRunner.FindEntries(command, scalarResult, setTotalCount, out totalCount);
        }

        public IDictionary<string, object> FindEntry(string commandText)
        {
            int totalCount;
            var result = FindEntries(commandText, false, false, out totalCount);
            return result == null ? null : result.FirstOrDefault();
        }

        public object FindScalar(string commandText)
        {
            int totalCount;
            var result = FindEntries(commandText, true, false, out totalCount);
            return result == null ? null : result.FirstOrDefault().Values.First();
        }

        public IDictionary<string, object> GetEntry(string collection, params object[] entryKey)
        {
            var entryKeyWithNames = new Dictionary<string, object>();
            var keyNames = _schema.FindTable(collection).GetKeyNames();
            for (int index = 0; index < keyNames.Count; index++)
            {
                entryKeyWithNames.Add(keyNames[index], entryKey.ElementAt(index));
            }
            return GetEntry(collection, entryKeyWithNames);
        }

        public IDictionary<string, object> GetEntry(string collection, IDictionary<string, object> entryKey)
        {
            var commandText = new ODataClientWithCommand(this, _schema).From(collection).Key(entryKey).CommandText;
            var command = HttpCommand.Get(commandText);
            _requestBuilder.AddCommandToRequest(command);
            return _requestRunner.GetEntry(command);
        }

        public IDictionary<string, object> InsertEntry(string collection, IDictionary<string, object> entryData, bool resultRequired)
        {
            var entryMembers = ParseEntryMembers(collection, entryData);

            var entry = ODataFeedReader.CreateDataElement(entryMembers.Properties);
            foreach (var associatedData in entryMembers.AssociationsByValue)
            {
                CreateLinkElement(entry, collection, associatedData);
            }

            var commandText = _schema.FindTable(collection).ActualName;
            var command = HttpCommand.Post(commandText, entryData, entry.ToString());
            _requestBuilder.AddCommandToRequest(command);
            var result = _requestRunner.InsertEntry(command, resultRequired);

            foreach (var associatedData in entryMembers.AssociationsByContentId)
            {
                var linkCommand = CreateLinkCommand(collection, associatedData.Key, 
                    ODataFeedReader.CreateLinkPath(command.ContentId), 
                    ODataFeedReader.CreateLinkPath(associatedData.Value));
                _requestBuilder.AddCommandToRequest(linkCommand);
                _requestRunner.InsertEntry(linkCommand, resultRequired);
            }

            return result;
        }

        public int UpdateEntries(string collection, string commandText, IDictionary<string, object> entryData)
        {
            return IterateEntries(collection, commandText, entryData, UpdateEntry);
        }

        public int UpdateEntry(string collection, IDictionary<string, object> entryKey, IDictionary<string, object> entryData)
        {
            var entryMembers = ParseEntryMembers(collection, entryData);
            return UpdateEntryPropertiesAndAssociations(collection, entryKey, entryData, entryMembers);
        }

        public int DeleteEntries(string collection, string commandText)
        {
            return IterateEntries(collection, commandText, null, (x,y,z) => DeleteEntry(x,y));
        }

        public int DeleteEntry(string collection, IDictionary<string, object> entryKey)
        {
            var commandText = new ODataClientWithCommand(this, _schema).From(collection).Key(entryKey).CommandText;
            var command = HttpCommand.Delete(commandText);
            _requestBuilder.AddCommandToRequest(command);
            return _requestRunner.DeleteEntry(command);
        }

        public void LinkEntry(string collection, IDictionary<string, object> entryKey, string linkName, IDictionary<string, object> linkedEntryKey)
        {
            var association = _schema.FindTable(collection).FindAssociation(linkName);
            var command = CreateLinkCommand(collection, linkName,
                new ODataClientWithCommand(this, _schema).From(collection).Key(entryKey).CommandText,
                new ODataClientWithCommand(this, _schema).From(association.ReferenceTableName).Key(linkedEntryKey).CommandText);
            _requestBuilder.AddCommandToRequest(command);
            _requestRunner.UpdateEntry(command);
        }

        public void UnlinkEntry(string collection, IDictionary<string, object> entryKey, string linkName)
        {
            var association = _schema.FindTable(collection).FindAssociation(linkName);
            var command = CreateUnlinkCommand(collection, linkName, new ODataClientWithCommand(this, _schema).From(collection).Key(entryKey).CommandText);
            _requestBuilder.AddCommandToRequest(command);
            _requestRunner.UpdateEntry(command);
        }

        public IEnumerable<IEnumerable<IEnumerable<KeyValuePair<string, object>>>> ExecuteFunction(string functionName, IDictionary<string, object> parameters)
        {
            var function = _schema.FindFunction(functionName);
            var command = new HttpCommand(function.HttpMethod.ToUpper(), new ODataClientWithCommand(this, _schema).Function(functionName).Parameters(parameters).CommandText);
            _requestBuilder.AddCommandToRequest(command);
            return _requestRunner.ExecuteFunction(command);
        }

        public string FormatFilter(string collectionName, dynamic filterExpression)
        {
            if (filterExpression is FilterExpression)
            {
                var clientWithCommand = new ODataClientWithCommand(this, _schema);
                string filter = (filterExpression as FilterExpression).Format(clientWithCommand, collectionName);
                return clientWithCommand.From(collectionName).Filter(filter).CommandText;
            }
            else
            {
                throw new InvalidOperationException("Unable to cast dynamic object to FilterExpression");
            }
        }

        private int IterateEntries(string collection, string commandText, IDictionary<string, object> entryData, 
            Func<string, IDictionary<string, object>, IDictionary<string, object>, int> func)
        {
            var entryKey = ExtractKeyFromCommandText(collection, commandText);
            if (entryKey != null)
            {
                return func(collection, entryKey, entryData);
            }
            else
            {
                var entries = new ODataClient(_urlBase).FindEntries(commandText);
                if (entries != null)
                {
                    var entryList = entries.ToList();
                    foreach (var entry in entryList)
                    {
                        func(collection, entry, entryData);
                    }
                    return entryList.Count;
                }
                else
                {
                    return 0;
                }
            }
        }

        private int UpdateEntryPropertiesAndAssociations(string collection, IDictionary<string, object> entryKey, IDictionary<string, object> entryData, EntryMembers entryMembers)
        {
            bool hasPropertiesToUpdate = entryMembers.Properties.Count > 0;
            bool merge = !hasPropertiesToUpdate || CheckMergeConditions(collection, entryKey, entryData);
            var commandText = new ODataClientWithCommand(this, _schema).From(collection).Key(entryKey).CommandText;

            var entryElement = ODataFeedReader.CreateDataElement(entryMembers.Properties);
            var unlinkAssociationNames = new List<string>();
            foreach (var associatedData in entryMembers.AssociationsByValue)
            {
                var association = _schema.FindTable(collection).FindAssociation(associatedData.Key);
                if (associatedData.Value != null)
                {
                    CreateLinkElement(entryElement, collection, associatedData);
                }
                else
                {
                    unlinkAssociationNames.Add(association.ActualName);
                }
            }

            var command = new HttpCommand(merge ? RestVerbs.MERGE : RestVerbs.PUT, commandText, entryData, entryElement.ToString());
            _requestBuilder.AddCommandToRequest(command);
            var result = _requestRunner.UpdateEntry(command);

            foreach (var associatedData in entryMembers.AssociationsByContentId)
            {
                var linkCommand = CreateLinkCommand(collection, associatedData.Key, 
                    ODataFeedReader.CreateLinkPath(command.ContentId), 
                    ODataFeedReader.CreateLinkPath(associatedData.Value));
                _requestBuilder.AddCommandToRequest(linkCommand);
                _requestRunner.UpdateEntry(linkCommand);
            }

            foreach (var associationName in unlinkAssociationNames)
            {
                UnlinkEntry(collection, entryKey, associationName);
            }

            return result;
        }

        private HttpCommand CreateLinkCommand(string collection, string associationName, string entryPath, string linkPath)
        {
            var linkEntry = ODataFeedReader.CreateLinkElement(linkPath);
            var linkMethod = _schema.FindTable(collection).FindAssociation(associationName).IsMultiple ? 
                RestVerbs.POST : 
                RestVerbs.PUT;

            var commandText = ODataFeedReader.CreateLinkCommand(entryPath, associationName);
            return new HttpCommand(linkMethod, commandText, null, linkEntry.ToString(), true);
        }

        private HttpCommand CreateUnlinkCommand(string collection, string associationName, string entryPath)
        {
            var commandText = ODataFeedReader.CreateLinkCommand(entryPath, associationName);
            return HttpCommand.Delete(commandText);
        }

        private void CreateLinkElement(XElement entry, string collection, KeyValuePair<string, object> associatedData)
        {
            if (associatedData.Value == null)
                return;

            var association = _schema.FindTable(collection).FindAssociation(associatedData.Key);
            var associatedKeyValues = GetLinkedEntryKeyValues(association.ReferenceTableName, associatedData);
            if (associatedKeyValues != null)
            {
                ODataFeedReader.AddDataLink(entry, association.ActualName, association.ReferenceTableName, associatedKeyValues);
            }
        }

        private IEnumerable<object> GetLinkedEntryKeyValues(string collection, KeyValuePair<string, object> entryData)
        {
            var entryProperties = GetLinkedEntryProperties(entryData.Value);
            var associatedKeyNames = _schema.FindTable(collection).GetKeyNames();
            var associatedKeyValues = new object[associatedKeyNames.Count()];
            for (int index = 0; index < associatedKeyNames.Count(); index++)
            {
                bool ok = entryProperties.TryGetValue(associatedKeyNames[index], out associatedKeyValues[index]);
                if (!ok)
                    return null;
            }
            return associatedKeyValues;
        }

        private IDictionary<string, object> GetLinkedEntryProperties(object entryData)
        {
            IDictionary<string, object> entryProperties = entryData as IDictionary<string, object>;
            if (entryProperties == null)
            {
                entryProperties = new Dictionary<string, object>();
                var entryType = entryData.GetType();
                foreach (var entryProperty in GetTypeProperties(entryType))
                {
                    entryProperties.Add(entryProperty.Name, GetTypeProperty(entryType, entryProperty.Name).GetValue(entryData, null));
                }
            }
            return entryProperties;
        }

        private EntryMembers ParseEntryMembers(string collection, IDictionary<string, object> entryData)
        {
            var entryMembers = new EntryMembers();

            var table = _schema.FindTable(collection);
            foreach (var item in entryData)
            {
                ParseEntryMember(table, item, entryMembers);
            }

            return entryMembers;
        }

        private void ParseEntryMember(Table table, KeyValuePair<string, object> item, EntryMembers entryMembers)
        {
            if (table.HasColumn(item.Key))
            {
                entryMembers.AddProperty(item.Key, item.Value);
            }
            else if (table.HasAssociation(item.Key))
            {
                var association = table.FindAssociation(item.Key);
                if (association.IsMultiple)
                {
                    var collection = item.Value as IEnumerable<object>;
                    if (collection != null)
                    {
                        foreach (var element in collection)
                        {
                            AddEntryAssociation(entryMembers, item.Key, element);
                        }
                    }
                }
                else
                {
                    AddEntryAssociation(entryMembers, item.Key, item.Value);
                }
            }
            else
            {
                throw new UnresolvableObjectException(item.Key, string.Format("No property or association found for {0}.", item.Key));
            }
        }

        private void AddEntryAssociation(EntryMembers entryMembers, string associationName, object associatedData)
        {
            int contentId = _requestBuilder.GetContentId(associatedData);
            if (contentId == 0)
            {
                entryMembers.AddAssociationByValue(associationName, associatedData);
            }
            else
            {
                entryMembers.AddAssociationByContentId(associationName, contentId);
            }
        }

        private bool CheckMergeConditions(string collection, IDictionary<string, object> entryKey, IDictionary<string, object> entryData)
        {
            var keyNames = _schema.FindTable(collection).GetKeyNames();
            foreach (var key in entryKey.Keys)
            {
                if (!keyNames.Contains(key) && !entryData.Keys.Contains(key))
                {
                    return true;
                }
            }
            return false;
        }
        
        private IDictionary<string, object> ExtractKeyFromCommandText(string collection, string commandText)
        {
            // TODO
            return null;
        }

        private IEnumerable<PropertyInfo> GetTypeProperties(Type type)
        {
#if NETFX_CORE
            return type.GetTypeInfo().DeclaredProperties;
#else
            return type.GetProperties();
#endif
        }

        private PropertyInfo GetTypeProperty(Type type, string propertyName)
        {
#if NETFX_CORE
            return type.GetTypeInfo().GetDeclaredProperty(propertyName);
#else
            return type.GetProperty(propertyName);
#endif
        }
    }
}