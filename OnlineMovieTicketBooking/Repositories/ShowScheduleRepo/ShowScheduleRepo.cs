using Microsoft.EntityFrameworkCore;
using OnlineMovieTicketBooking.DAL;
using OnlineMovieTicketBooking.Models;
using OnlineMovieTicketBooking.ViewModels;

namespace OnlineMovieTicketBooking.Repositories.ShowScheduleRepo
{
    public class ShowScheduleRepo : IShowScheduleRepo
    {
        OnlineMovieTicketBookingDBContext _context;

        public ShowScheduleRepo(OnlineMovieTicketBookingDBContext context)
        {
            _context = context;
        }
        public async Task<int> CreateShowSchedule(ShowScheduleModel showSchedule, decimal regularPrice, decimal premiumPrice, decimal reclinerPrice)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var movie = await _context.Movies
                    .Where(x => x.MovieId == showSchedule.MovieId).FirstOrDefaultAsync();

                if (movie == null)
                    return 0;
                if (!movie.MovIsActive)
                    return 0;
                if (showSchedule.ShowDate < movie.MovRelDate)
                    return 0;
                var screen = await _context.Screens
                .Where(x => x.ScreenId == showSchedule.ScreenId).FirstOrDefaultAsync();

                if (screen == null || !screen.ScreenIsActive)
                    return 0;

                showSchedule.ShowEndTime = showSchedule.ShowStartTime.AddMinutes(movie.MovDuration).AddMinutes(30);
                var overlappingShow = await _context.ShowSchedules.AnyAsync
                   (x => x.ScreenId == showSchedule.ScreenId &&
                   x.ShowDate == showSchedule.ShowDate &&
                   showSchedule.ShowStartTime < x.ShowEndTime &&
                   showSchedule.ShowEndTime > x.ShowStartTime);

                if (overlappingShow)
                    return 0;

                _context.ShowSchedules.Add(showSchedule);
                await _context.SaveChangesAsync();
                List<ShowSeatPriceModel> prices = new List<ShowSeatPriceModel>
                {
                    new ShowSeatPriceModel
                    {
                        ShowScheduleId = showSchedule.ShowScheduleId,
                        SeatType = SeatType.Regular,
                        ShowSeatPrice = regularPrice
                    },
                    new ShowSeatPriceModel
                    {
                        ShowScheduleId = showSchedule.ShowScheduleId,
                        SeatType = SeatType.Premium,
                        ShowSeatPrice = premiumPrice
                    },
                    new ShowSeatPriceModel
                    {
                        ShowScheduleId = showSchedule.ShowScheduleId,
                        SeatType = SeatType.Recliner,
                        ShowSeatPrice = reclinerPrice
                    }
                };
                await _context.ShowSeatPrices.AddRangeAsync(prices);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return 1;
            }
            catch
            {
                await transaction.RollbackAsync();
                return 0;
            }
        }

        public async Task<int> DeleteShowSchedule(ShowScheduleModel showSchedule)
        {
            try
            {
                _context.ShowSchedules.Remove(showSchedule);
                await _context.SaveChangesAsync();
                return 1;
            }
            catch (DbUpdateException)
            {
                return 0;
            }
            
        }

        public async Task<List<ShowScheduleModel>> DisplayShowSchedules()
        {
            return await _context.ShowSchedules.Include(x => x.Movie).Include(x => x.Screen).Include(x => x.ShowSeatPrices).ToListAsync();
        }

        public async Task<ShowScheduleModel> GetShowScheduleById(int showScheduleId)
        {
            return await _context.ShowSchedules
                .AsNoTracking()
                .Include(x => x.Movie)
                .Include(x => x.Screen)
                .Include(x => x.ShowSeatPrices)
                .Where(x => x.ShowScheduleId == showScheduleId).FirstOrDefaultAsync();
        }
        public async Task<int> UpdateShowSchedule(ShowScheduleModel showSchedule, decimal regularPrice, decimal premiumPrice, decimal reclinerPrice)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var existingShow = await _context.ShowSchedules
                    .FirstOrDefaultAsync(x => x.ShowScheduleId == showSchedule.ShowScheduleId);

                if (existingShow == null)
                    return 0;

                var prices = await _context.ShowSeatPrices
                    .Where(x => x.ShowScheduleId == showSchedule.ShowScheduleId)
                    .ToListAsync();

                existingShow.ShowIsActive = showSchedule.ShowIsActive;
                var regular = prices.FirstOrDefault(x => x.SeatType == SeatType.Regular);
                var premium = prices.FirstOrDefault(x => x.SeatType == SeatType.Premium);
                var recliner = prices.FirstOrDefault(x => x.SeatType == SeatType.Recliner);

                if (regular != null)
                    regular.ShowSeatPrice = regularPrice;

                if (premium != null)
                    premium.ShowSeatPrice = premiumPrice;

                if (recliner != null)
                    recliner.ShowSeatPrice = reclinerPrice;

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return 1;
            }
            catch
            {
                await transaction.RollbackAsync();
                return 0;
            }
        }
        public async Task<List<MovieModel>> GetMovies()
        {
            return await _context.Movies
                .Where(x => x.MovIsActive)
                .ToListAsync();
        }

        public async Task<List<ScreenModel>> GetScreens()
        {
            return await _context.Screens
                .Where(x => x.ScreenIsActive)
                .ToListAsync();
        }
    }
}
