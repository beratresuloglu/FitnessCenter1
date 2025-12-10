using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using FitnessCenterWebApplication.Models.Entities;

namespace FitnessCenterWebApplication.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
        
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Database oluştur
            await context.Database.MigrateAsync();

            // Roller oluştur
            await CreateRoles(roleManager);

            // Admin kullanıcısı oluştur
            await CreateAdminUser(userManager);

            // Örnek veriler oluştur
            if (!context.GymCenters.Any())
            {
                await SeedData(context, userManager);
            }
        }

        private static async Task CreateRoles(RoleManager<IdentityRole> roleManager)
        {
            string[] roles = { "Admin", "Member", "Trainer" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        private static async Task CreateAdminUser(UserManager<ApplicationUser> userManager)
        {
            var adminEmail = "b231210050@sakarya.edu.tr"; // ÖĞRENCİ NUMARANIZI BURAYA YAZIN!
            var adminPassword = "sau";

            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "Admin",
                    LastName = "User",
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedDate = DateTime.Now
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }

        private static async Task SeedData(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            // 1. Spor Salonu Oluştur
            var gymCenter = new GymCenter
            {
                Name = "Sakarya Fitness Center",
                Address = "Serdivan, Sakarya",
                Phone = "0264 123 45 67",
                Email = "info@sakaryafitness.com",
                OpenTime = new TimeSpan(6, 0, 0),    // 06:00
                CloseTime = new TimeSpan(23, 0, 0),   // 23:00
                Description = "Modern ekipmanlarla donatılmış, profesyonel antrenörler eşliğinde spor yapabileceğiniz fitness center.",
                IsActive = true
            };
            context.GymCenters.Add(gymCenter);
            await context.SaveChangesAsync();

            // 2. Hizmetler Oluştur
            var services = new List<Service>
            {
                new Service
                {
                    Name = "Personal Training",
                    Description = "Birebir kişisel antrenörlük hizmeti",
                    DurationMinutes = 60,
                    Price = 200,
                    GymCenterId = gymCenter.Id,
                    IsActive = true
                },
                new Service
                {
                    Name = "Yoga",
                    Description = "Grup yoga dersleri",
                    DurationMinutes = 45,
                    Price = 100,
                    GymCenterId = gymCenter.Id,
                    IsActive = true
                },
                new Service
                {
                    Name = "Pilates",
                    Description = "Pilates group class",
                    DurationMinutes = 45,
                    Price = 100,
                    GymCenterId = gymCenter.Id,
                    IsActive = true
                },
                new Service
                {
                    Name = "Crossfit",
                    Description = "Yoğun crossfit antrenmanı",
                    DurationMinutes = 60,
                    Price = 150,
                    GymCenterId = gymCenter.Id,
                    IsActive = true
                },
                new Service
                {
                    Name = "Beslenme Danışmanlığı",
                    Description = "Kişiye özel beslenme programı",
                    DurationMinutes = 30,
                    Price = 250,
                    GymCenterId = gymCenter.Id,
                    IsActive = true
                }
            };
            context.Services.AddRange(services);
            await context.SaveChangesAsync();

            // 3. Örnek Üye Oluştur
            var memberUser = new ApplicationUser
            {
                UserName = "member@test.com",
                Email = "member@test.com",
                FirstName = "Ahmet",
                LastName = "Yılmaz",
                EmailConfirmed = true,
                IsActive = true
            };
            await userManager.CreateAsync(memberUser, "Member123!");
            await userManager.AddToRoleAsync(memberUser, "Member");

            var member = new Member
            {
                FirstName = "Ahmet",
                LastName = "Yılmaz",
                Phone = "0532 111 22 33",
                Email = "member@test.com",
                DateOfBirth = new DateTime(1995, 5, 15),
                Gender = "Erkek",
                Height = 175,
                Weight = 80,
                FitnessGoal = "Kilo Verme",
                UserId = memberUser.Id,
                IsActive = true,
                JoinDate = DateTime.Now,
                MembershipExpiry = DateTime.Now.AddYears(1)
            };
            context.Members.Add(member);
            await context.SaveChangesAsync();

            // 4. Antrenörler Oluştur
            var trainer1User = new ApplicationUser
            {
                UserName = "trainer1@test.com",
                Email = "trainer1@test.com",
                FirstName = "Can",
                LastName = "Demir",
                EmailConfirmed = true,
                IsActive = true
            };
            await userManager.CreateAsync(trainer1User, "Trainer123!");
            await userManager.AddToRoleAsync(trainer1User, "Trainer");

            var trainer1 = new Trainer
            {
                FirstName = "Can",
                LastName = "Demir",
                Phone = "0533 222 33 44",
                Email = "trainer1@test.com",
                Specialization = "Personal Training, Crossfit",
                Bio = "10 yıllık deneyime sahip profesyonel antrenör",
                ExperienceYears = 10,
                GymCenterId = gymCenter.Id,
                UserId = trainer1User.Id,
                IsActive = true
            };
            context.Trainers.Add(trainer1);

            var trainer2User = new ApplicationUser
            {
                UserName = "trainer2@test.com",
                Email = "trainer2@test.com",
                FirstName = "Zeynep",
                LastName = "Kaya",
                EmailConfirmed = true,
                IsActive = true
            };
            await userManager.CreateAsync(trainer2User, "Trainer123!");
            await userManager.AddToRoleAsync(trainer2User, "Trainer");

            var trainer2 = new Trainer
            {
                FirstName = "Zeynep",
                LastName = "Kaya",
                Phone = "0534 333 44 55",
                Email = "trainer2@test.com",
                Specialization = "Yoga, Pilates",
                Bio = "Yoga ve Pilates uzmanı, sertifikalı eğitmen",
                ExperienceYears = 7,
                GymCenterId = gymCenter.Id,
                UserId = trainer2User.Id,
                IsActive = true
            };
            context.Trainers.Add(trainer2);
            await context.SaveChangesAsync();

            // 5. Antrenör-Hizmet İlişkileri
            var trainerServices = new List<TrainerService>
            {
                new TrainerService { TrainerId = trainer1.Id, ServiceId = services[0].Id, IsActive = true }, // Personal Training
                new TrainerService { TrainerId = trainer1.Id, ServiceId = services[3].Id, IsActive = true }, // Crossfit
                new TrainerService { TrainerId = trainer2.Id, ServiceId = services[1].Id, IsActive = true }, // Yoga
                new TrainerService { TrainerId = trainer2.Id, ServiceId = services[2].Id, IsActive = true }  // Pilates
            };
            context.TrainerServices.AddRange(trainerServices);

            // 6. Antrenör Müsaitlik Saatleri
            var availabilities = new List<TrainerAvailability>();

            // Trainer 1 - Hafta içi her gün
            for (int i = 1; i <= 5; i++)
            {
                availabilities.Add(new TrainerAvailability
                {
                    TrainerId = trainer1.Id,
                    DayOfWeek = (DayOfWeek)i,
                    StartTime = new TimeSpan(9, 0, 0),
                    EndTime = new TimeSpan(18, 0, 0),
                    IsAvailable = true
                });
            }

            // Trainer 2 - Hafta içi her gün
            for (int i = 1; i <= 5; i++)
            {
                availabilities.Add(new TrainerAvailability
                {
                    TrainerId = trainer2.Id,
                    DayOfWeek = (DayOfWeek)i,
                    StartTime = new TimeSpan(10, 0, 0),
                    EndTime = new TimeSpan(19, 0, 0),
                    IsAvailable = true
                });
            }

            context.TrainerAvailabilities.AddRange(availabilities);
            await context.SaveChangesAsync();
        }
    }
}