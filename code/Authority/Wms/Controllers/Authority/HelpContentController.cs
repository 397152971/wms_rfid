﻿using System.Web.Mvc;
using System.Text;
using System.Web.Routing;
using System.Web.Script.Serialization;
using THOK.Wms.Bll.Interfaces;
using Microsoft.Practices.Unity;
using THOK.WebUtil;
using THOK.Authority.DbModel;
using THOK.Authority.Bll.Interfaces;
using System;

namespace Authority.Controllers.Authority
{
    public class HelpContentController : Controller
    {
        [Dependency]
        public IHelpContentService HelpContentService { get; set; }

        //
        // GET: /HelpContent/
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
        // POST: /HelpContent/Create
        [HttpPost]
        public ActionResult Create(HelpContent helpContent)
        {
            string strResult = string.Empty;
            bool bResult = HelpContentService.Add(helpContent, out strResult);
            string msg = bResult ? "新增成功" : "新增失败";
            return Json(JsonMessageHelper.getJsonMessage(bResult, msg, strResult), "text", JsonRequestBehavior.AllowGet);
        }

        //
        // GET: /HelpContent/Details/

        public ActionResult Details(int page, int rows, string QueryString, string Value)
        {
            if (QueryString == null)
            {
                QueryString = "ContentName";
            }
            if (Value == null)
            {
                Value = "";
            }
            var product = HelpContentService.GetDetails(page, rows, QueryString, Value);
            return Json(product, "text", JsonRequestBehavior.AllowGet);
        }
        // GET: /HelpContent/Details/

        public ActionResult Details2(int page, int rows, FormCollection collection)
        {
            string ContentCode = collection["ContentCode"] ?? "";
            string ContentName = collection["ContentName"] ?? "";
            string ModuleName = collection["ModuleName"] ?? "";
            string NodeType = collection["NodeType"] ?? "";
            string FatherNodeID = collection["FatherNodeID"] ?? "";
            string ModuleID = collection["ModuleID"] ?? "";
            string NodeOrder = collection["NodeOrder"] ?? "";
            string IsActive = collection["IsActive"] ?? "";
            string UpdateTime = collection["UpdateTime"] ?? "";
            var users = HelpContentService.GetDetails2(page, rows, ContentCode, ContentName, ModuleName, NodeType, FatherNodeID, NodeOrder, ModuleID, IsActive, UpdateTime);
            return Json(users, "text", JsonRequestBehavior.AllowGet);
        }
        // POST: /HelpContent/Edit/
        [HttpPost]
        public ActionResult Edit(string ID, string ContentCode, string ContentName, string ContentPath, string NodeType , string FatherNodeID,string ModuleID,int NodeOrder, string IsActive)
        {
            string strResult = string.Empty;
            bool bResult = HelpContentService.Save(ID, ContentCode, ContentName, ContentPath, FatherNodeID, ModuleID, NodeOrder, IsActive, out strResult);
            string msg = bResult ? "修改成功" : "修改失败";
            return Json(JsonMessageHelper.getJsonMessage(bResult, msg, null), "text", JsonRequestBehavior.AllowGet);
        }
        //
        // POST: /UnitList/Delete/

        [HttpPost]
        public ActionResult Delete(string ContentCode)
        {
            bool bResult = HelpContentService.Delete(ContentCode);
            string msg = bResult ? "删除成功" : "删除失败";
            return Json(JsonMessageHelper.getJsonMessage(bResult, msg, null), "text", JsonRequestBehavior.AllowGet);
        }

    }
}