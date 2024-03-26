﻿using Simple.OData.Client;
using Simple.OData.Tests.Client.Entities;
using Xunit;

namespace Simple.OData.Tests.Client.Core;

public class SettingsRegistrationTests
{
	[Fact]
	public void RegisterContainer()
	{
		var settings = new ODataClientSettings { BaseUri = new Uri("http://localhost") };

		settings.TypeCache.Register<Animal>();

		// NB Stored under AbsoluteUri so need trailing /
		var typeCache = TypeCaches.TypeCache("http://localhost/", null);

		Assert.Equal("DynamicProperties", typeCache.DynamicContainerName(typeof(Animal)));
	}

	[Fact]
	public void RegisterNamedContainer()
	{
		var settings = new ODataClientSettings { BaseUri = new Uri("http://localhost") };

		settings.TypeCache.Register<Animal>("Foo");

		// NB Stored under AbsoluteUri so need trailing /
		var typeCache = TypeCaches.TypeCache("http://localhost/", null);

		Assert.Equal("Foo", typeCache.DynamicContainerName(typeof(Animal)));
	}
}