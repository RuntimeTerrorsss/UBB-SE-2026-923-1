// <copyright file="DatabaseConfig.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingBoardGames
{
    public static class DatabaseConfig
    {
        // change this to your server
        public const string ConnectionString = "Server=(localdb)\\MSSQLLocalDB;Database=MergedBoardGamesDb;Trusted_Connection=True;TrustServerCertificate=True;";
    }
}
