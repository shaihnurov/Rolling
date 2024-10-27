using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Rolling.Models;
using ServerSignal.Models;

namespace ServerSignal.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly ApplicationContextDb _db;
        
        public UserController(ApplicationContextDb db)
        {
            _db = db;
        }
        
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var cars = await _db.UserModels.ToListAsync();
            return View(cars);
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _db.UserModels.FirstOrDefaultAsync(x => x.Id == id);

            if (user == null)
            {
                return RedirectToAction("Index");
            }

            var editUser = new UserModel
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Age = user.Age,
                Level = user.Level,
                Permission = user.Permission,
            };

            return View("Edit", editUser);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(UserModel userModel)
        {
            var sql = @"UPDATE UserModels SET Name = @Name, Email = @Email, Age = @Age, Level = @Level, Permission = @Permission WHERE Id = @Id";

            try
            {
                await _db.Database.ExecuteSqlRawAsync(sql, 
                    new SqlParameter("@Name", userModel.Name),
                    new SqlParameter("@Email", userModel.Email),
                    new SqlParameter("@Age", userModel.Age),
                    new SqlParameter("@Level", userModel.Level),
                    new SqlParameter("@Permission", userModel.Permission),
                    new SqlParameter("@Id", userModel.Id)
                );
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine("Ошибка обновления данных. Попробуйте еще раз");
                return View("Edit", userModel);
            }

            return RedirectToAction("Index");
        }
        [HttpPost]
        public async Task<IActionResult> Delete(UserModel userModel)
        {
            var existingUser = await _db.UserModels.FirstOrDefaultAsync(x => x.Id == userModel.Id);

            if (existingUser == null)
            {
                Console.WriteLine("Пользователь не найден");
                return RedirectToAction("Index");
            }

            var sql = "DELETE FROM UserModels WHERE Id = @Id";
        
            try
            {
                var result = await _db.Database.ExecuteSqlRawAsync(sql, new SqlParameter("@Id", userModel.Id));
            
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