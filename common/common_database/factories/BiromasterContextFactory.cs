using Microsoft.EntityFrameworkCore;
using si.birokrat.next.common_database.contexts;
using System;
using System.Collections.Generic;
using System.Text;

namespace si.birokrat.next.common_database.factories {
    public static class BiromasterContextFactory {
        public static BiromasterDbContext Create(string connectionString) {
            if (string.IsNullOrEmpty(connectionString)) {
                return null;
            }

            var optionsBuilder = new DbContextOptionsBuilder<BiromasterDbContext>();
            optionsBuilder.UseSqlServer(connectionString);
            return new BiromasterDbContext(optionsBuilder.Options) {
            };
        }
    }
}
