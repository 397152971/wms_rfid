﻿using THOK.Authority.DbModel;

namespace THOK.Authority.Bll.Interfaces
{
    public interface IHelpContentService : IService<HelpContent>
    {
        bool Add(HelpContent helpContent, out string strResult);

        bool Save(string ID, string ContentCode, string ContentName, string ContentPath, string FatherNodeID, string ModuleID, int NodeOrder, string IsActive, out string strResult);

        object GetDetails2(int page, int rows, string ContentCode, string ContentName, string ModuleName, string NodeType, string FatherNodeID, string NodeOrder, string ModuleID, string IsActive, string UpdateTime);

        object GetDetails(int page, int rows, string QueryString, string Value);

        bool Delete(string ContentCode);
    }
}