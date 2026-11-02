using Estoque.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Estoque.Infra.Data.Context
{
    // The ApplicationDbContext class is a custom implementation of the DbContext class provided by Entity Framework Core.
    // It serves as the primary class responsible for interacting with the database and managing the entity models.
    public class ApplicationDbContext: DbContext
    {

        // Constructor that takes DbContextOptions and passes them to the base DbContext class
        // This allows for configuration of the database context, such as specifying the database provider and connection string.
        // The options parameter is typically configured in the Startup.cs or Program.cs file of the application.
        // The base(options) call ensures that the DbContext is properly initialized with the provided options.
        // This constructor is essential for dependency injection and allows the context to be used in various parts of the application.
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSet properties represent the tables in the database. Each DbSet corresponds to a specific entity class.
        public DbSet<Produto> Produtos { get; set; }

        // The OnModelCreating method is overridden to configure the model using the ModelBuilder API.
        // This method is called when the model for a derived context has been initialized, but before the model has been locked down and used to initialize the context.
        // In this implementation, it calls the base method and applies configurations from the assembly where the ApplicationDbContext class is defined.
        // This allows for a clean separation of configuration logic into separate classes, making the code more maintainable and organized.
        // The ApplyConfigurationsFromAssembly method scans the specified assembly for classes that implement IEntityTypeConfiguration<T> and applies their configurations to the model.
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }

    }
}

