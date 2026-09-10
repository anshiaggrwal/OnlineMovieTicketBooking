using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OnlineMovieTicketBooking.Controllers;
using OnlineMovieTicketBooking.Models;
using OnlineMovieTicketBooking.Repositories.TheatreRepo;
using OnlineMovieTicketBooking.ViewModels;

namespace OnlineMovieTicketBookingTests
{
    public class TheatreControllerTests
    {
        [Fact]
        public async Task Index_ReturnsViewWithTheatres()
        {
            var theatreRepo = new Mock<ITheatreRepo>();
            var mapper = new Mock<IMapper>();

            var theatres = new List<TheatreModel>
            {
                new TheatreModel
                {
                    TheatreId = 1,
                    TheaName = "PVR",
                    TheaCIty = "Delhi"
                }
            };

            var theatreVM = new List<TheatreVM>
            {
                new TheatreVM
                {
                    TheatreId = 1,
                    TheaName = "PVR",
                    TheaCIty = "Delhi"
                }
            };

            theatreRepo.Setup(x => x.DisplayTheatres())  //mock starts
                .ReturnsAsync(theatres);

            mapper.Setup(x => x.Map<List<TheatreVM>>(theatres))
                .Returns(theatreVM);

            var controller = new TheatreController(
                theatreRepo.Object,
                mapper.Object);

            var result = await controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<List<TheatreVM>>(viewResult.Model);

            Assert.Single(model);
            Assert.Equal("PVR", model[0].TheaName);
            Assert.Equal("Delhi", model[0].TheaCIty);
        }
    }
}