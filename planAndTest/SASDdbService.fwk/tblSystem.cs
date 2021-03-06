﻿using commonLib;
using models.fwk.SD;
using SASDdb.entity.fwk;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SASDdbService.fwk
{
    public class tblSystem : SASDdbBase
    {
        public tblSystem() : base()
        {
        }
        public tblSystem(SASDdbContext db) : base(db)
        {
        }
        public IQueryable<systems> getAll()
        {
            IQueryable<systems> ret;
            ret = (from a in db.systems
                   select a).AsQueryable();
            return ret;
        }
        public IQueryable<systemDisp> getAllDisp()
        {
            IQueryable<systemDisp> ret;
            ret = (from a in db.systems
                   join b in db.project on a.projectId equals b.projectId
                   select new systemDisp
                   {
                       systemId=a.systemId,
                       createtime=a.createtime,
                       systemName=a.systemName,
                       systemDescription=a.systemDescription,
                       systemType=a.systemType,
                       projectId=a.projectId,
                       projectName=b.projectName,
                       systemGroupName=a.systemGroupName
                   }).AsQueryable();
            return ret;
        }
        public systems getById(string systemId)
        {
            systems ret;
            var qry = (from a in db.systems
                       where a.systemId == new Guid(systemId)
                       select a).FirstOrDefault();
            ret = qry;
            return ret;
        }
        public systems getByNameId(string systemName, string projectId)
        {
            systems ret;
            var qry = (from a in db.systems
                       where a.systemName == systemName &&
                            a.projectId == new Guid( projectId)
                       select a).FirstOrDefault();
            ret = qry;
            return ret;
        }
        public string Add(systems newSystem)
        {
            string ret = "";
            db.systems.Add(newSystem);
            return ret;
        }
        public string Update(systems updateSystem)
        {
            string ret = "";
            var aSystem = db.systems.SingleOrDefault(x => x.systemId
                == updateSystem.systemId);
            if (aSystem != null)
            {
                aSystem = reflectionUtl.assign<systems, systems>(
                    aSystem, updateSystem);
                db.Entry(aSystem).State = EntityState.Modified;
            }
            else
                throw new Exception($"system {updateSystem.systemId}" +
                    $" not found");
            return ret;
        }
        public string Delete(systems deleteSystem)
        {
            string ret = "";
            db.Entry(deleteSystem).State = EntityState.Deleted;
            // need save changes
            return ret;
        }
        public string Delete(string systemId)
        {
            string ret;
            systems deleteSystem = getById(systemId);
            ret = Delete(deleteSystem);
            return ret;
        }
    }
}
