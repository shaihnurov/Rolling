using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Rolling.Models;
using ServerSignal.Models;

namespace ServerSignal.Controllers
{
    [Authorize]
    public class RentalCarsController : Controller
    {
        private readonly ApplicationContextDb _db;
        
        public RentalCarsController(ApplicationContextDb db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var cars = await _db.CarsRentalModels.ToListAsync();
            return View(cars);
        }
        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Add(CarsRentalModels carsRentalModels, IFormFile imageFile)
        {
            if (imageFile != null)
            {
                using (var ms = new MemoryStream())
                {
                    await imageFile.CopyToAsync(ms);
                    carsRentalModels.Image = ms.ToArray();
                }
            }
            
            var rentalCar = new CarsRentalModels
            {
                Id = Guid.NewGuid(),
                Mark = carsRentalModels.Mark,
                Model = carsRentalModels.Model,
                Years = carsRentalModels.Years,
                Color = carsRentalModels.Color,
                HorsePower = carsRentalModels.HorsePower,
                Mileage = carsRentalModels.Mileage,
                Engine = carsRentalModels.Engine,
                Price = carsRentalModels.Price,
                City = carsRentalModels.City,
                Status = carsRentalModels.Status,
                Image = carsRentalModels.Image
            };
            
            await _db.CarsRentalModels.AddAsync(rentalCar);
            await _db.SaveChangesAsync();
            
            return RedirectToAction("Index");
        }
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var car = await _db.CarsRentalModels.FirstOrDefaultAsync(x => x.Id == id);

            if (car == null)
            {
                return RedirectToAction("Index");
            }

            var editCar = new CarsRentalModels
            {
                Id = car.Id,
                Mark = car.Mark,
                Model = car.Model,
                Years = car.Years,
                Color = car.Color,
                HorsePower = car.HorsePower,
                Mileage = car.Mileage,
                Engine = car.Engine,
                Price = car.Price,
                City = car.City,
                Status = car.Status,
            };

            return View("Edit", editCar);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(CarsRentalModels carsRentalModels, IFormFile imageFile)
        {
            if (imageFile != null)
            {
                using (var ms = new MemoryStream())
                {
                    await imageFile.CopyToAsync(ms);
                    carsRentalModels.Image = ms.ToArray();
                }
            }
            
            var sql = @"UPDATE CarsRentalModels SET Mark = @Mark, Model = @Model, Years = @Years, Color = @Color, HorsePower = @HorsePower, Mileage = @Mileage, Engine = @Engine, Price = @Price, City = @City, Status = @Status, Image = @Image WHERE Id = @Id";

            try
            {
                await _db.Database.ExecuteSqlRawAsync(sql, 
                    new SqlParameter("@Mark", carsRentalModels.Mark),
                    new SqlParameter("@Model", carsRentalModels.Model),
                    new SqlParameter("@Years", carsRentalModels.Years),
                    new SqlParameter("@Color", carsRentalModels.Color),
                    new SqlParameter("@HorsePower", carsRentalModels.HorsePower),
                    new SqlParameter("@Mileage", carsRentalModels.Mileage),
                    new SqlParameter("@Engine", carsRentalModels.Engine),
                    new SqlParameter("@Price", carsRentalModels.Price),
                    new SqlParameter("@City", carsRentalModels.City),
                    new SqlParameter("@Status", carsRentalModels.Status),
                    new SqlParameter("@Id", carsRentalModels.Id),
                    new SqlParameter("@Image", carsRentalModels.Image != null ? new SqlParameter("@Image", carsRentalModels.Image) : new SqlParameter("@Image", DBNull.Value))
                );
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine("Ошибка обновления данных. Попробуйте еще раз " + ex.Message);
                return View("Edit", carsRentalModels);
            }

            return RedirectToAction("Index");
        }
        [HttpPost]
        public async Task<IActionResult> Delete(CarsRentalModels carsRentalModels)
        {
            var existingCar = await _db.CarsRentalModels.FirstOrDefaultAsync(x => x.Id == carsRentalModels.Id);

            if (existingCar == null)
            {
                Console.WriteLine("Автомобиль не найден");
                return RedirectToAction("Index");
            }

            var sql = "DELETE FROM CarsRentalModels WHERE Id = @Id";
        
            try
            {
                var result = await _db.Database.ExecuteSqlRawAsync(sql, new SqlParameter("@Id", carsRentalModels.Id));
            
                if (result > 0)
                {
                    return RedirectToAction("Index");
                }

                Console.WriteLine("Ошибка удаления данных. Запись не найдена или уже удалена.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Произошла ошибка при удалении. Попробуйте еще раз.");
            }

            return RedirectToAction("Index");
        }
    }
}