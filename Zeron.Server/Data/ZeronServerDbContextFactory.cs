// Zeron - Scheduled Task Application for Windows OS
// Copyright (c) 2019 Jiowcl. All rights reserved.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Zeron.Server.ZCore;

namespace Zeron.Server.Data
{
    /// <summary>
    /// ZeronServerDbContextFactory
    /// </summary>
    public class ZeronServerDbContextFactory : IDesignTimeDbContextFactory<ZeronServerDbContext>
    {
        /// <summary>
        /// CreateDbContext
        /// </summary>
        /// <param name="args"></param>
        /// <returns>Returns ZeronServerDbContext.</returns>
        public ZeronServerDbContext CreateDbContext(
            string[] args)
        {
            DbContextOptions<ZeronServerDbContext> options = new DbContextOptionsBuilder<ZeronServerDbContext>()
                .UseSqlite("Data Source=Data/" + AppEnums.DbFileName)
                .Options;

            return new ZeronServerDbContext(options);
        }
    }
}
