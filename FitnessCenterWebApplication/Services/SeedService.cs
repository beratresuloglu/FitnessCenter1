using FitnessCenterWebApplication.Data;
using FitnessCenterWebApplication.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterWebApplication.Services
{
    public class SeedService
    {
        public static async Task SeedDatabase(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<SeedService>>();

            try
            {
                // Ensure the database is ready
                logger.LogInformation("Ensuring the database is created");
                await context.Database.MigrateAsync();

                // Add roles
                logger.LogInformation("Seeding roles");
                await AddRolesAsync(roleManager, "Admin");
                await AddRolesAsync(roleManager, "User");
                await AddRolesAsync(roleManager, "Trainer");

                // Add Admin user
                logger.LogInformation("Seeding admin user");
                var adminEmail = "b231210050@sakarya.edu.tr";
                User adminUser = null;

                if (await userManager.FindByEmailAsync(adminEmail) == null)
                {
                    adminUser = new User
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        NormalizedUserName = adminEmail.ToUpper(),
                        FirstName = "Admin",
                        LastName = "User",
                        EmailConfirmed = true,
                        IsActive = true,
                        CreatedDate = DateTime.Now,
                        SecurityStamp = Guid.NewGuid().ToString()
                    };
                    var result = await userManager.CreateAsync(adminUser, "sau");
                    if (result.Succeeded)
                    {
                        logger.LogInformation("Assigning admin role to the admin user");
                        await userManager.AddToRoleAsync(adminUser, "Admin");
                    }
                    else
                    {
                        logger.LogError("Failed to create admin user: {Errors}",
                            string.Join(", ", result.Errors.Select(e => e.Description)));
                        return;
                    }
                }
                else
                {
                    adminUser = await userManager.FindByEmailAsync(adminEmail);
                }

                // Seed GymCenter
                logger.LogInformation("Seeding gym center");
                await SeedGymCenter(context);

                // Seed Sample Users
                logger.LogInformation("Seeding sample users");
                var sampleUsers = await SeedSampleUsers(userManager, logger);

                // Seed Members
                logger.LogInformation("Seeding members");
                await SeedMembers(context, sampleUsers);

                // Seed Services
                logger.LogInformation("Seeding services");
                await SeedServices(context);

                // Seed Trainers
                logger.LogInformation("Seeding trainers");
                await SeedTrainers(context, userManager, logger);

                // Seed Trainer Availabilities
                logger.LogInformation("Seeding trainer availabilities");
                await SeedTrainerAvailabilities(context);

                // Seed Trainer Services
                logger.LogInformation("Seeding trainer services");
                await SeedTrainerServices(context);

                // Seed Appointments
                logger.LogInformation("Seeding appointments");
                await SeedAppointments(context);

                // Seed Workout Plans
                logger.LogInformation("Seeding workout plans");
                await SeedWorkoutPlans(context);

                logger.LogInformation("Database seeding completed successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding the database");
                throw;
            }
        }

        private static async Task AddRolesAsync(RoleManager<IdentityRole> roleManager, string roleName)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var result = await roleManager.CreateAsync(new IdentityRole(roleName));
                if (!result.Succeeded)
                {
                    throw new Exception($"Failed to create role '{roleName}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        }

        private static async Task SeedGymCenter(AppDbContext context)
        {
            if (!await context.GymCenters.AnyAsync())
            {
                var gymCenter = new GymCenter
                {
                    Name = "Tiger Fitness Center",
                    Address = "Serdivan, Sakarya Üniversitesi Kampüsü, 54050 Serdivan/Sakarya",
                    Phone = "+90 264 295 5454",
                    Email = "info@tiger.com.tr",
                    OpenTime = new TimeSpan(6, 0, 0),
                    CloseTime = new TimeSpan(23, 0, 0),
                    Description = "Modern ekipmanlar ve uzman kadrosuyla hizmet veren spor merkezi",
                    IsActive = true,
                    CreatedDate = DateTime.Now
                };

                context.GymCenters.Add(gymCenter);
                await context.SaveChangesAsync();
            }
        }

        private static async Task<List<User>> SeedSampleUsers(UserManager<User> userManager, ILogger logger)
        {
            var users = new List<User>();
            var sampleEmails = new[]
            {
                "ahmet.yilmaz@gmail.com",
                "ayse.kaya@gmail.com",
                "mehmet.demir@gmail.com",
                "fatma.celik@gmail.com",
                "ali.ozturk@gmail.com"
            };

            foreach (var email in sampleEmails)
            {
                if (await userManager.FindByEmailAsync(email) == null)
                {
                    var names = email.Split('@')[0].Split('.');
                    var user = new User
                    {
                        UserName = email,
                        Email = email,
                        NormalizedUserName = email.ToUpper(),
                        FirstName = char.ToUpper(names[0][0]) + names[0].Substring(1),
                        LastName = char.ToUpper(names[1][0]) + names[1].Substring(1),
                        EmailConfirmed = true,
                        IsActive = true,
                        CreatedDate = DateTime.Now,
                        SecurityStamp = Guid.NewGuid().ToString()
                    };

                    var result = await userManager.CreateAsync(user, "User123!");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, "User");
                        users.Add(user);
                    }
                }
                else
                {
                    users.Add(await userManager.FindByEmailAsync(email));
                }
            }

            return users;
        }

        private static async Task SeedMembers(AppDbContext context, List<User> users)
        {
            if (!await context.Members.AnyAsync() && users.Any())
            {
                var members = new List<Member>
                {
                    new Member
                    {
                        FirstName = users[0].FirstName,
                        LastName = users[0].LastName,
                        Phone = "+90 532 123 4567",
                        Email = users[0].Email,
                        DateOfBirth = new DateTime(1995, 5, 15),
                        Address = "Adapazarı, Sakarya",
                        EmergencyContact = "+90 532 999 8888",
                        Gender = "Erkek",
                        Height = 180,
                        Weight = 85,
                        FitnessGoal = "Kas geliştirme",
                        MedicalConditions = "Yok",
                        IsActive = true,
                        JoinDate = DateTime.Now.AddMonths(-6),
                        MembershipExpiry = DateTime.Now.AddMonths(6),
                        UserId = users[0].Id
                    },
                    new Member
                    {
                        FirstName = users[1].FirstName,
                        LastName = users[1].LastName,
                        Phone = "+90 533 234 5678",
                        Email = users[1].Email,
                        DateOfBirth = new DateTime(1992, 8, 20),
                        Address = "Serdivan, Sakarya",
                        EmergencyContact = "+90 533 888 7777",
                        Gender = "Kadın",
                        Height = 165,
                        Weight = 58,
                        FitnessGoal = "Kilo verme",
                        MedicalConditions = "Yok",
                        IsActive = true,
                        JoinDate = DateTime.Now.AddMonths(-3),
                        MembershipExpiry = DateTime.Now.AddMonths(9),
                        UserId = users[1].Id
                    },
                    new Member
                    {
                        FirstName = users[2].FirstName,
                        LastName = users[2].LastName,
                        Phone = "+90 534 345 6789",
                        Email = users[2].Email,
                        DateOfBirth = new DateTime(1988, 3, 10),
                        Address = "Adapazarı, Sakarya",
                        EmergencyContact = "+90 534 777 6666",
                        Gender = "Erkek",
                        Height = 175,
                        Weight = 90,
                        FitnessGoal = "Dayanıklılık artırma",
                        MedicalConditions = "Yok",
                        IsActive = true,
                        JoinDate = DateTime.Now.AddMonths(-8),
                        MembershipExpiry = DateTime.Now.AddMonths(4),
                        UserId = users[2].Id
                    }
                };

                context.Members.AddRange(members);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedServices(AppDbContext context)
        {
            if (!await context.Services.AnyAsync())
            {
                var gymCenter = await context.GymCenters.FirstAsync();

                var services = new List<Service>
                {
                    new Service
                    {
                        Name = "Kişisel Antrenörlük",
                        Description = "Bire bir özel antrenörlük hizmeti",
                        DurationMinutes = 60,
                        Price = 250.00m,
                        IsActive = true,
                        CreatedDate = DateTime.Now,
                        GymCenterId = gymCenter.Id
                    },
                    new Service
                    {
                        Name = "Yoga Dersi",
                        Description = "Grup yoga dersleri",
                        DurationMinutes = 90,
                        Price = 150.00m,
                        IsActive = true,
                        CreatedDate = DateTime.Now,
                        GymCenterId = gymCenter.Id
                    },
                    new Service
                    {
                        Name = "Pilates",
                        Description = "Pilates mat egzersizleri",
                        DurationMinutes = 60,
                        Price = 180.00m,
                        IsActive = true,
                        CreatedDate = DateTime.Now,
                        GymCenterId = gymCenter.Id
                    },
                    new Service
                    {
                        Name = "Crossfit",
                        Description = "Yoğun grup crossfit antrenmanı",
                        DurationMinutes = 75,
                        Price = 200.00m,
                        IsActive = true,
                        CreatedDate = DateTime.Now,
                        GymCenterId = gymCenter.Id
                    },
                    new Service
                    {
                        Name = "Zumba",
                        Description = "Eğlenceli zumba dansı dersleri",
                        DurationMinutes = 60,
                        Price = 120.00m,
                        IsActive = true,
                        CreatedDate = DateTime.Now,
                        GymCenterId = gymCenter.Id
                    },
                    new Service
                    {
                        Name = "Beslenme Danışmanlığı",
                        Description = "Kişiye özel beslenme programı hazırlama",
                        DurationMinutes = 45,
                        Price = 300.00m,
                        IsActive = true,
                        CreatedDate = DateTime.Now,
                        GymCenterId = gymCenter.Id
                    }
                };

                context.Services.AddRange(services);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedTrainers(AppDbContext context, UserManager<User> userManager, ILogger logger)
        {
            if (!await context.Trainers.AnyAsync())
            {
                var gymCenter = await context.GymCenters.FirstAsync();

                // Trainer users
                var trainerEmails = new[]
                {
                    "emre.yilmaz@fitlife.com.tr",
                    "zeynep.kara@fitlife.com.tr",
                    "can.demir@fitlife.com.tr"
                };

                var trainerUsers = new List<User>();
                foreach (var email in trainerEmails)
                {
                    if (await userManager.FindByEmailAsync(email) == null)
                    {
                        var names = email.Split('@')[0].Split('.');
                        var user = new User
                        {
                            UserName = email,
                            Email = email,
                            NormalizedUserName = email.ToUpper(),
                            FirstName = char.ToUpper(names[0][0]) + names[0].Substring(1),
                            LastName = char.ToUpper(names[1][0]) + names[1].Substring(1),
                            EmailConfirmed = true,
                            IsActive = true,
                            CreatedDate = DateTime.Now,
                            SecurityStamp = Guid.NewGuid().ToString()
                        };

                        var result = await userManager.CreateAsync(user, "Trainer123!");
                        if (result.Succeeded)
                        {
                            await userManager.AddToRoleAsync(user, "Trainer");
                            trainerUsers.Add(user);
                        }
                    }
                    else
                    {
                        trainerUsers.Add(await userManager.FindByEmailAsync(email));
                    }
                }

                var trainers = new List<Trainer>
                {
                    new Trainer
                    {
                        FirstName = trainerUsers[0].FirstName,
                        LastName = trainerUsers[0].LastName,
                        Phone = "+90 535 111 2233",
                        Email = trainerUsers[0].Email,
                        Specialization = "Kişisel Antrenörlük, Kas Geliştirme",
                        Bio = "10 yıllık tecrübeye sahip profesyonel antrenör",
                        ExperienceYears = 10,
                        IsActive = true,
                        HireDate = DateTime.Now.AddYears(-3),
                        CreatedDate = DateTime.Now,
                        GymCenterId = gymCenter.Id,
                        UserId = trainerUsers[0].Id
                    },
                    new Trainer
                    {
                        FirstName = trainerUsers[1].FirstName,
                        LastName = trainerUsers[1].LastName,
                        Phone = "+90 536 222 3344",
                        Email = trainerUsers[1].Email,
                        Specialization = "Yoga, Pilates",
                        Bio = "Sertifikalı yoga ve pilates eğitmeni",
                        ExperienceYears = 7,
                        IsActive = true,
                        HireDate = DateTime.Now.AddYears(-2),
                        CreatedDate = DateTime.Now,
                        GymCenterId = gymCenter.Id,
                        UserId = trainerUsers[1].Id
                    },
                    new Trainer
                    {
                        FirstName = trainerUsers[2].FirstName,
                        LastName = trainerUsers[2].LastName,
                        Phone = "+90 537 333 4455",
                        Email = trainerUsers[2].Email,
                        Specialization = "Crossfit, Fonksiyonel Antrenman",
                        Bio = "Crossfit Level 2 sertifikalı antrenör",
                        ExperienceYears = 5,
                        IsActive = true,
                        HireDate = DateTime.Now.AddYears(-1),
                        CreatedDate = DateTime.Now,
                        GymCenterId = gymCenter.Id,
                        UserId = trainerUsers[2].Id
                    }
                };

                context.Trainers.AddRange(trainers);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedTrainerAvailabilities(AppDbContext context)
        {
            if (!await context.TrainerAvailabilities.AnyAsync())
            {
                var trainers = await context.Trainers.ToListAsync();
                var availabilities = new List<TrainerAvailability>();

                foreach (var trainer in trainers)
                {
                    // Pazartesi - Cuma arası müsaitlik
                    for (int day = 1; day <= 5; day++)
                    {
                        availabilities.Add(new TrainerAvailability
                        {
                            TrainerId = trainer.Id,
                            DayOfWeek = (DayOfWeek)day,
                            StartTime = new TimeSpan(9, 0, 0),
                            EndTime = new TimeSpan(12, 0, 0),
                            IsActive = true,
                            CreatedDate = DateTime.Now
                        });

                        availabilities.Add(new TrainerAvailability
                        {
                            TrainerId = trainer.Id,
                            DayOfWeek = (DayOfWeek)day,
                            StartTime = new TimeSpan(14, 0, 0),
                            EndTime = new TimeSpan(18, 0, 0),
                            IsActive = true,
                            CreatedDate = DateTime.Now
                        });
                    }

                    // Cumartesi
                    availabilities.Add(new TrainerAvailability
                    {
                        TrainerId = trainer.Id,
                        DayOfWeek = DayOfWeek.Saturday,
                        StartTime = new TimeSpan(10, 0, 0),
                        EndTime = new TimeSpan(14, 0, 0),
                        IsActive = true,
                        CreatedDate = DateTime.Now
                    });
                }

                context.TrainerAvailabilities.AddRange(availabilities);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedTrainerServices(AppDbContext context)
        {
            if (!await context.TrainerServices.AnyAsync())
            {
                var trainers = await context.Trainers.ToListAsync();
                var services = await context.Services.ToListAsync();
                var trainerServices = new List<TrainerService>();

                // İlk antrenör - Kişisel antrenörlük, Crossfit
                trainerServices.Add(new TrainerService
                {
                    TrainerId = trainers[0].Id,
                    ServiceId = services.First(s => s.Name == "Kişisel Antrenörlük").Id,
                    AssignedDate = DateTime.Now,
                    IsActive = true
                });
                trainerServices.Add(new TrainerService
                {
                    TrainerId = trainers[0].Id,
                    ServiceId = services.First(s => s.Name == "Crossfit").Id,
                    AssignedDate = DateTime.Now,
                    IsActive = true
                });

                // İkinci antrenör - Yoga, Pilates
                trainerServices.Add(new TrainerService
                {
                    TrainerId = trainers[1].Id,
                    ServiceId = services.First(s => s.Name == "Yoga Dersi").Id,
                    AssignedDate = DateTime.Now,
                    IsActive = true
                });
                trainerServices.Add(new TrainerService
                {
                    TrainerId = trainers[1].Id,
                    ServiceId = services.First(s => s.Name == "Pilates").Id,
                    AssignedDate = DateTime.Now,
                    IsActive = true
                });

                // Üçüncü antrenör - Crossfit, Zumba
                trainerServices.Add(new TrainerService
                {
                    TrainerId = trainers[2].Id,
                    ServiceId = services.First(s => s.Name == "Crossfit").Id,
                    AssignedDate = DateTime.Now,
                    IsActive = true
                });
                trainerServices.Add(new TrainerService
                {
                    TrainerId = trainers[2].Id,
                    ServiceId = services.First(s => s.Name == "Zumba").Id,
                    AssignedDate = DateTime.Now,
                    IsActive = true
                });

                context.TrainerServices.AddRange(trainerServices);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedAppointments(AppDbContext context)
        {
            if (!await context.Appointments.AnyAsync())
            {
                var members = await context.Members.ToListAsync();
                var trainers = await context.Trainers.ToListAsync();
                var services = await context.Services.ToListAsync();

                if (members.Any() && trainers.Any() && services.Any())
                {
                    var appointments = new List<Appointment>
                    {
                        new Appointment
                        {
                            MemberId = members[0].Id,
                            TrainerId = trainers[0].Id,
                            ServiceId = services[0].Id,
                            AppointmentDate = DateTime.Today.AddDays(1),
                            StartTime = new TimeSpan(10, 0, 0),
                            EndTime = new TimeSpan(11, 0, 0),
                            Status = 0, // Pending
                            IsApproved = false,
                            TotalPrice = services[0].Price,
                            Notes = "İlk randevu",
                            CreatedDate = DateTime.Now
                        },
                        new Appointment
                        {
                            MemberId = members[1].Id,
                            TrainerId = trainers[1].Id,
                            ServiceId = services[1].Id,
                            AppointmentDate = DateTime.Today.AddDays(2),
                            StartTime = new TimeSpan(14, 0, 0),
                            EndTime = new TimeSpan(15, 30, 0),
                            Status = (AppointmentStatus)1, // Approved
                            IsApproved = true,
                            ApprovedDate = DateTime.Now,
                            TotalPrice = services[1].Price,
                            CreatedDate = DateTime.Now.AddDays(-1)
                        },
                        new Appointment
                        {
                            MemberId = members[0].Id,
                            TrainerId = trainers[2].Id,
                            ServiceId = services[3].Id,
                            AppointmentDate = DateTime.Today.AddDays(-3),
                            StartTime = new TimeSpan(16, 0, 0),
                            EndTime = new TimeSpan(17, 15, 0),
                            Status = (AppointmentStatus)2, // Completed
                            IsApproved = true,
                            ApprovedDate = DateTime.Now.AddDays(-3),
                            TotalPrice = services[3].Price,
                            CreatedDate = DateTime.Now.AddDays(-5)
                        }
                    };

                    context.Appointments.AddRange(appointments);
                    await context.SaveChangesAsync();
                }
            }
        }

        private static async Task SeedWorkoutPlans(AppDbContext context)
        {
            if (!await context.WorkoutPlans.AnyAsync())
            {
                var members = await context.Members.ToListAsync();

                if (members.Any())
                {
                    var workoutPlans = new List<WorkoutPlan>
                    {
                        new WorkoutPlan
                        {
                            MemberId = members[0].Id,
                            Title = "Kas Geliştirme Programı",
                            AIRecommendation = "Boy: 180cm, Kilo: 85kg için önerilen 12 haftalık kas geliştirme programı. Haftada 5 gün antrenman, protein ağırlıklı beslenme.",
                            UserInputData = "{\"height\":180,\"weight\":85,\"goal\":\"muscle_gain\"}",
                            PlanType = "Muscle Building",
                            DurationWeeks = 12,
                            IsActive = true,
                            CreatedDate = DateTime.Now.AddDays(-10)
                        },
                        new WorkoutPlan
                        {
                            MemberId = members[1].Id,
                            Title = "Kilo Verme Programı",
                            AIRecommendation = "Boy: 165cm, Kilo: 58kg için önerilen 8 haftalık kilo verme ve tonlama programı. Kardio + direnç egzersizleri kombinasyonu.",
                            UserInputData = "{\"height\":165,\"weight\":58,\"goal\":\"weight_loss\"}",
                            PlanType = "Weight Loss",
                            DurationWeeks = 8,
                            IsActive = true,
                            CreatedDate = DateTime.Now.AddDays(-5)
                        }
                    };

                    context.WorkoutPlans.AddRange(workoutPlans);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}