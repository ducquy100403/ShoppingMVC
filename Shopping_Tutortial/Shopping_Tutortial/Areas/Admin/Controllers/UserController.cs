using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Shopping_Tutortial.Models;
using Shopping_Tutortial.Models.ViewModels;
using Shopping_Tutortial.Repository;

namespace Shopping_Tutortial.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/User")]
	[Authorize(Roles = "Admin")]
	public class UserController : Controller
    {
        private readonly UserManager<AppUserModel> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly DataContext _dataContext;

        public UserController(UserManager<AppUserModel> userManager, RoleManager<IdentityRole> roleManager, DataContext dataContext)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _dataContext = dataContext;
        }

        // Hiển thị danh sách user cùng với vai trò
        [HttpGet]
        [Route("Index")]  // Route rõ ràng cho trang Index: /Admin/User/Index
        public async Task<IActionResult> Index(int pg = 1)
        {
            const int pageSize = 10;  // Số lượng người dùng trên mỗi trang
            if (pg < 1)
            {
                pg = 1;
            }

            // Đếm tổng số người dùng
            int recsCount = await _userManager.Users.CountAsync();

            // Tạo đối tượng phân trang
            var pager = new Paginate(recsCount, pg, pageSize);

            // Tính số lượng bản ghi cần bỏ qua
            var recSkip = (pg - 1) * pageSize;

            // Lấy danh sách người dùng và vai trò với phân trang
            var userWithRoles = await (from u in _dataContext.Users
                                       join ur in _dataContext.UserRoles on u.Id equals ur.UserId into userRoles
                                       from ur in userRoles.DefaultIfEmpty()
                                       join r in _dataContext.Roles on ur.RoleId equals r.Id into roles
                                       from r in roles.DefaultIfEmpty()
                                       select new UserWithRoleViewModel
                                       {
                                           User = u,
                                           Roles = r != null ? new List<string> { r.Name } : new List<string>()
                                       })
                                       .Skip(recSkip)
                                       .Take(pageSize)
                                       .ToListAsync();

            // Truyền đối tượng phân trang vào ViewBag để sử dụng trong view
            ViewBag.Pager = pager;

            // Trả về view với danh sách người dùng đã phân trang
            return View(userWithRoles);
        }

        // Tạo user mới
        [HttpGet]
        [Route("Create")]
        public async Task<IActionResult> Create()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            ViewBag.Roles = new SelectList(roles, "Id", "Name");
            return View(new AppUserModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Create")]
        public async Task<IActionResult> Create(AppUserModel user)
        {
            if (ModelState.IsValid)
            {
                var createUserResult = await _userManager.CreateAsync(user, user.PasswordHash);
                if (createUserResult.Succeeded)
                {
                    var createUser = await _userManager.FindByEmailAsync(user.Email);
                    var role = await _roleManager.FindByIdAsync(user.RoleId);

                    if (role == null)
                    {
                        ModelState.AddModelError(string.Empty, "Role không tồn tại.");
                        var roles = await _roleManager.Roles.ToListAsync();
                        ViewBag.Roles = new SelectList(roles, "Id", "Name");
                        return View(user);
                    }

                    var addToRoleResult = await _userManager.AddToRoleAsync(createUser, role.Name);
                    if (!addToRoleResult.Succeeded)
                    {
                        foreach (var error in addToRoleResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                        var roles = await _roleManager.Roles.ToListAsync();
                        ViewBag.Roles = new SelectList(roles, "Id", "Name");
                        return View(user);
                    }

                    TempData["success"] = "User đã được tạo thành công.";
                    return RedirectToAction("Index", "User");
                }
                else
                {
                    foreach (var error in createUserResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }

            var rolesList = await _roleManager.Roles.ToListAsync();
            ViewBag.Roles = new SelectList(rolesList, "Id", "Name");
            return View(user);
        }

        // Sửa thông tin và vai trò của user
        [HttpGet]
        [Route("Edit")]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            // Lấy user theo id
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Lấy tất cả các vai trò của user
            var userRoles = await _userManager.GetRolesAsync(user);

            // Lấy tất cả các vai trò trong hệ thống
            var roles = await _roleManager.Roles.ToListAsync();

            // Tìm vai trò hiện tại của user và chọn nó trong danh sách SelectList
            var selectedRole = roles.FirstOrDefault(r => userRoles.Contains(r.Name));

            // Gán vai trò hiện tại vào SelectList
            ViewBag.Roles = new SelectList(roles, "Id", "Name", selectedRole?.Id);

            return View(user);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Edit")]
        public async Task<IActionResult> Edit(string id, AppUserModel user)
        {
            var existingUser = await _userManager.FindByIdAsync(id);
            if (existingUser == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Cập nhật thông tin cá nhân của user
                existingUser.UserName = user.UserName;
                existingUser.Email = user.Email;
                existingUser.PhoneNumber = user.PhoneNumber;

                var updateUserResult = await _userManager.UpdateAsync(existingUser);
                if (updateUserResult.Succeeded)
                {
                    // Lấy vai trò hiện tại của user và xóa chúng
                    var userRoles = await _userManager.GetRolesAsync(existingUser);
                    var removeRolesResult = await _userManager.RemoveFromRolesAsync(existingUser, userRoles);

                    if (!removeRolesResult.Succeeded)
                    {
                        ModelState.AddModelError(string.Empty, "Không thể xóa các vai trò hiện tại.");
                        return View(user);
                    }

                    // Tìm vai trò mới
                    var role = await _roleManager.FindByIdAsync(user.RoleId);
                    if (role == null)
                    {
                        ModelState.AddModelError(string.Empty, "Vai trò không tồn tại.");
                        return View(user);
                    }

                    // Gán vai trò mới cho user
                    var addToRoleResult = await _userManager.AddToRoleAsync(existingUser, role.Name);
                    if (!addToRoleResult.Succeeded)
                    {
                        ModelState.AddModelError(string.Empty, "Không thể gán vai trò mới cho user.");
                        return View(user);
                    }

                    TempData["success"] = "User đã được cập nhật thành công.";
                    return RedirectToAction("Index", "User");
                }

                foreach (var error in updateUserResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // Nếu ModelState không hợp lệ hoặc có lỗi, hiển thị lại vai trò hiện tại
            var rolesList = await _roleManager.Roles.ToListAsync();
            var userRolesAfterUpdate = await _userManager.GetRolesAsync(existingUser); // Lấy lại role sau update

            var selectedRoleAfterUpdate = rolesList.FirstOrDefault(r => userRolesAfterUpdate.Contains(r.Name));
            ViewBag.Roles = new SelectList(rolesList, "Id", "Name", selectedRoleAfterUpdate?.Id);

            return View(user);
        }


        // Xóa user
        [HttpGet]
        [Route("Delete/{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var deleteResult = await _userManager.DeleteAsync(user);
            if (!deleteResult.Succeeded)
            {
                foreach (var error in deleteResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View("Error");
            }

            TempData["success"] = "User đã bị xoá.";
            return RedirectToAction("Index", "User");
        }
    }
}
