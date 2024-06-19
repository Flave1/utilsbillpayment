using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using VendTech.Areas.Api.Controllers.b2b;

namespace Vendtech.Test
{
    [TestClass]
    public class BsbUsersEndpointsTest
    {
        private HttpClient _client;
        private Mock<ICalculatorService> _mockCalService;
        private b2btestController _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockCalService = new Mock<ICalculatorService>();
            _controller = new b2btestController(_mockCalService.Object);
            _client = new HttpClient { BaseAddress = new Uri("http://localhost:56549/api/b2btest") };
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        //[TestMethod]
        public async Task Create_user_then_return_ok()
        {
            //Arrange
            var result = 2;
            // result can also be an object
            _mockCalService.Setup(x => x.Add(1, 1)).Returns(result);

            // Act
            HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Get, "/get/1");
            var response = await _client.SendAsync(httpRequest);
            var stringResult = await response.Content.ReadAsStringAsync();

            //var response = await _client.GetAsync("/get/1");

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var returned = await response.Content.ReadAsStreamAsync();
            Assert.AreEqual("2", returned);
        }
    }
}
