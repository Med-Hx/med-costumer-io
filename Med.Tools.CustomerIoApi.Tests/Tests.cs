using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Med.Tools.CustomerIoApi.Tests
{
    [TestClass]
    public class Tests
    {
        [TestMethod]
        public void FullTest()
        {
            var client = new CustomerIoClient("addSiteId", "AddApiKey");

            var data = new Dictionary<string, string>();

            data.Add("names", "Test");
            data.Add("surnames", "Client");

            Assert.IsTrue(client.IdentifyCustomer("test-customer", "test@test.com", DateTime.UtcNow, data).Result);

            var eventData = new Dictionary<string, string>();

            eventData.Add("id", "Test Event");

            Assert.IsTrue(client.TrackEvent("test-customer", "test_event", eventData).Result);

            Assert.IsTrue(client.DeleteCustomer("test-customer").Result);
        }
    }
}