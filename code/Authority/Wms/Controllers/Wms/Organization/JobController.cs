﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Practices.Unity;
using THOK.Wms.Bll.Interfaces;
using THOK.Wms.DbModel;
using THOK.WebUtil;

namespace Authority.Controllers.Wms.Organization
{
    public class JobController : Controller
    {
        [Dependency]
        public IJobService JobService { get; set; }
        //
        // GET: /Job/

        public ActionResult Index(string moduleID)
        {
            ViewBag.hasSearch = true;
            ViewBag.hasAdd = true;
            ViewBag.hasEdit = true;
            ViewBag.hasDelete = true;
            ViewBag.hasPrint = true;
            ViewBag.hasHelp = true;
            ViewBag.ModuleID = moduleID;
            return View();
        }

        //
        // GET: /Job/Details/

        public ActionResult Details(int page, int rows, FormCollection collection)
        {
            string JobCode = collection["JobCode"] ?? "";
            string JobName = collection["JobName"] ?? "";
            string IsActive = collection["IsActive"] ?? "";
            var job = JobService.GetDetails(page, rows, JobCode, JobName, IsActive);
            return Json(job, "text", JsonRequestBehavior.AllowGet);
        }

        //
        // POST: /Job/Create/

        [HttpPost]
        public ActionResult Create(Job job)
        {
            bool bResult = JobService.Add(job);
            string msg = bResult ? "新增成功" : "新增失败";
            return Json(JsonMessageHelper.getJsonMessage(bResult, msg, null), "text", JsonRequestBehavior.AllowGet);
        }

        //
        // POST: /Job/Edit/5

        public ActionResult Edit(Job job)
        {
            bool bResult = JobService.Save(job);
            string msg = bResult ? "修改成功" : "修改失败";
            return Json(JsonMessageHelper.getJsonMessage(bResult, msg, null), "text", JsonRequestBehavior.AllowGet);
        }

        //
        // POST: /Job/Delete/

        [HttpPost]
        public ActionResult Delete(string jobId)
        {
            string error = null;
            bool bResult = false;
            try
            {
                bResult = JobService.Delete(jobId);
            }
            catch (Exception)
            {
                error = "已在使用";
            }
            string msg = bResult ? "删除成功" : "删除失败";
            return Json(JsonMessageHelper.getJsonMessage(bResult, msg, error), "text", JsonRequestBehavior.AllowGet);
        }

        // POST: /Job/GetJob/
        public ActionResult GetJob(int page, int rows, string queryString, string value)
        {
            if (queryString == null)
            {
                queryString = "JobCode";
            }
            if (value == null)
            {
                value = "";
            }
            var job = JobService.GetJob(page, rows, queryString, value);
            return Json(job, "text", JsonRequestBehavior.AllowGet);
        }
    }
}
