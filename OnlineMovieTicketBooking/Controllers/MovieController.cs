using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineMovieTicketBooking.Models;
using OnlineMovieTicketBooking.Repositories.MovieRepo;
using OnlineMovieTicketBooking.ViewModels;

namespace OnlineMovieTicketBooking.Controllers
{
    public class MovieController : Controller
    {
        IMovieRepo _movieRepo;
        IMapper _mapper;

        public MovieController(IMovieRepo movieRepo, IMapper mapper)
        {
            _movieRepo = movieRepo;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var movies = await _movieRepo.DisplayMovies();
            var movieVM = _mapper.Map<List<MovieVM>>(movies);
            return View(movieVM);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            return View();
        }


        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(MovieVM movieVM)
        {
            if (!ModelState.IsValid)
            {
                return View(movieVM);
            }

            var movie = _mapper.Map<MovieModel>(movieVM);

            if (await _movieRepo.CreateMovie(movie) > 0)
            {
                return RedirectToAction("Index");
            }
            else
            {
                return View(movieVM);
            }

        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var movie = await _movieRepo.GetMovieById(id);

            if (movie == null)
                return NotFound();

            var movieVM = _mapper.Map<MovieVM>(movie);

            return View(movieVM);
        }


        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var movie = await _movieRepo.GetMovieById(id);
            if (movie == null)
            {
                return NotFound();
            }
            else
            {
                var movieVM = _mapper.Map<MovieVM>(movie);
                return View(movieVM);
            }

        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Edit(MovieVM movieVM)
        {
            if (!ModelState.IsValid)
            {
                return View(movieVM);
            }
            var existingMovie = await _movieRepo.GetMovieById(movieVM.MovieId);

            if (existingMovie == null)
            {
                return NotFound();
            }

            if (existingMovie.MovIsActive && !movieVM.MovIsActive)
            {
                ModelState.AddModelError("MovIsActive", "An active movie cannot be deactivated.");
                return View(movieVM);
            }
            var movie = _mapper.Map<MovieModel>(movieVM);

            if (await _movieRepo.UpdateMovie(movie) > 0)
            {
                return RedirectToAction("Index");
            }
            else
            {
                return View(movieVM);
            }

        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var movie = await _movieRepo.GetMovieById(id);
            if (movie == null)
            {
                return NotFound();
            }
            else
            {
                var movieVM = _mapper.Map<MovieVM>(movie);
                return View(movieVM);
            }

        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Delete(MovieVM movieVM)
        {
            var movie = _mapper.Map<MovieModel>(movieVM);

            if (await _movieRepo.DeleteMovie(movie) > 0)
            {
                return RedirectToAction("Index");
            }
            else
            {
                ModelState.Clear();
                ModelState.AddModelError("", "Movie cannot be deleted because this movie is scheduled in multiple theatres and multiple bookings exist for this movie.");
                return View(movieVM);
            }

        }

    }
}