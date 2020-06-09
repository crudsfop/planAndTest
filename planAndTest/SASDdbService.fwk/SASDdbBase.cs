﻿//using Microsoft.EntityFrameworkCore.Storage;
using Newtonsoft.Json.Serialization;
using SASDdb.entity.fwk;
//using SASDdb.entity.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Text;

namespace SASDdbService
{
    public class SASDdbBase
    {
        protected SASDdbContext db;
        public SASDdbBase()
        {
            db = new SASDdbContext();
        }
        public DbContextTransaction BeginTransaction()
        {
            DbContextTransaction ret = 
                db.Database.BeginTransaction();
            return ret;
        }
        public virtual string SaveChanges()
        {
            string ret = "";
            db.SaveChanges();
            return ret;
        }
    }
}
