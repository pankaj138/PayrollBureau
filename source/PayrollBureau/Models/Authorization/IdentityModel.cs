﻿using System.ComponentModel.DataAnnotations.Schema;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Infrastructure.Annotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace PayrollBureau.Models.Authorization
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);

            // Add custom user claims here
            return userIdentity;

    }
        public string Name { get; set; }
    }

    public class ApplicationRole : IdentityRole
    {
        public ApplicationRole() : base() { }

        public ApplicationRole(string name)
            : base(name)
        {
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext() : base(ConfigurationManager.ConnectionStrings["DefaultConnection"].ToString(), throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        static ApplicationDbContext()
        {
            //Database.SetInitializer<ApplicationDbContext>(null);
            Database.SetInitializer(new CustomInitializer());
            Create().Database.Initialize(true);
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var user = modelBuilder.Entity<ApplicationUser>();

            user.Property(u => u.UserName)
               .IsRequired()
               .HasMaxLength(256)
               .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("UserNameIndex") { IsUnique = true, Order = 1 }));
        }

        //protected override DbEntityValidationResult ValidateEntity(DbEntityEntry entityEntry, IDictionary<object, object> items)
        //{
        //    if (entityEntry != null && entityEntry.State == EntityState.Added)
        //    {
        //        var errors = new List<DbValidationError>();
        //        var user = entityEntry.Entity as ApplicationUser;

        //        if (user != null)
        //        {
        //            if (Users.Any(u => string.Equals(u.UserName, user.UserName) && u.OrganisationId == user.OrganisationId))
        //                errors.Add(new DbValidationError("User", string.Format("Username {0} is already taken for OrganisationId {1}", user.UserName, user.OrganisationId)));

        //            if (Users.Any(u => string.Equals(u.Email, user.Email) && u.OrganisationId == user.OrganisationId))
        //                errors.Add(new DbValidationError("User", string.Format("Email Address {0} is already taken for OrganisationId {1}", user.UserName, user.OrganisationId)));
        //        }
        //        else
        //        {
        //            var role = entityEntry.Entity as IdentityRole;

        //            if (role != null && this.Roles.Any(r => string.Equals(r.Name, role.Name)))
        //                errors.Add(new DbValidationError("Role", string.Format("Role {0} already exists", role.Name)));
        //        }

        //        if (errors.Any())
        //            return new DbEntityValidationResult(entityEntry, errors);
        //    }

        //    return new DbEntityValidationResult(entityEntry, new List<DbValidationError>());
        //}


        public class CustomInitializer : IDatabaseInitializer<ApplicationDbContext>
        {
            public void InitializeDatabase(ApplicationDbContext context)
            {
                var userManager = new ApplicationUserManager(new UserStore<ApplicationUser>(context));
                var roleManager = new ApplicationRoleManager(new RoleStore<ApplicationRole>(context));

                if (!userManager.Users.Any(u => u.UserName == "superadmin@itsupportlimited.com"))
                {
                    var user = new ApplicationUser
                    {
                        UserName = "superadmin@itsupportlimited.com",
                        Email = "superadmin@itsupportlimited.com",
                        EmailConfirmed = true,
                    };

                    var roleId = roleManager.Roles.FirstOrDefault(r => r.Name == "SuperUser").Id;
                    user.Roles.Add(new IdentityUserRole { UserId = user.Id, RoleId = roleId });
                    var result =userManager.Create(user, "Inland12!");
                }
            }
        }
    }
}