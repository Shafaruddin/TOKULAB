using IRIS.CrmConnector.API.CRM.DAO;
using IRIS.CrmConnector.API.CRM.DTO.Input;

namespace IRIS.CrmConnector.API.Mappers
{
    public static class UpdateCaseAutoSaveDtoMapper
    {
        public static UpdateCaseInputDto ToUpdateCaseInputDto(this UpdateCaseAutoSaveDao dao, Guid cat1Default, Guid cat2Default, Guid cat3Default)
        {
            UpdateCaseInputDto dto = new();
            dto.CaseID = dao.CaseID; //required
            dto.CustomerGUID = (Guid)dao.CustomerGUID; //required
            dto.CaseTitle = dao.CaseTitle ?? "Auto save"; //required
            dto.CaseDetails = dao.CaseDetails ?? "Auto save. No details input by agent"; //required
            dto.SystemAuthenticated = dao.SystemAuthenticated ?? false;
            dto.ManualVerification = dao.ManualVerification ?? false;
            dto.IPCCCallExtensionID = dao.IPCCCallExtensionID ?? new Guid(); //required
            dto.Owner = dao.Owner ?? "Auto save"; //required
            dto.CaseCategory1 = dao.CaseCategory1 ?? cat1Default; //required
            dto.CaseCategory2 = dao.CaseCategory2 ?? cat2Default; //required
            dto.CaseCategory3 = dao.CaseCategory3 ?? cat3Default; //required
            dto.AdHocCriteria = dao.AdHocCriteria;
            dto.FollowupAction = dao.FollowupAction ?? "Auto save";
            dto.ContactMode = dao.ContactMode ?? ContactModeInputEnum.Inbound;
            dto.ModifiedBy = dao.ModifiedBy?.Replace(@"\\",@"\") ?? "AUTOSAVE"; //required
            dto.FollowupRequired = dao.FollowupRequired ?? false; //check angular
            dto.CaseStatus = dao.CaseStatus ?? Westcon.CrmClient.Model.CaseStatusInputEnum.InProgress;
            dto.Resolution = dao.Resolution ?? "";
            dto.TriggeredBy = dao.TriggeredBy?.Replace(@"\\", @"\") ?? "AUTOSAVE"; //required
            dto.TriggeredFrom = dao.TriggeredFrom ?? "AUTOSAVE"; //required
            return dto;
        }

        public static UpdateCaseAutoSaveDao FromUpdateCaseAutoSaveInputDto(this UpdateCaseAutoSaveInputDto dto, string caseId)
        {
            var dao = new UpdateCaseAutoSaveDao();
            dao.CaseID = caseId;
            dao.CustomerGUID = dto.CustomerGUID;
            dao.CaseTitle = $"{dto.CaseTitle} (Autosave)";
            dao.CaseDetails = dto.CaseDetails;
            dao.SystemAuthenticated = dto.SystemAuthenticated;
            dao.ManualVerification = dto.ManualVerification;
            dao.IPCCCallExtensionID = dto.IPCCCallExtensionID;
            dao.Owner = dto.Owner;
            dao.CaseCategory1 = dto.CaseCategory1;
            dao.CaseCategory2 = dto.CaseCategory2;
            dao.CaseCategory3 = dto.CaseCategory3;
            dao.AdHocCriteria = dto.AdHocCriteria;
            dao.FollowupAction = dto.FollowupAction;
            dao.ContactMode = dto.ContactMode;
            dao.ModifiedBy = dto.ModifiedBy;
            dao.FollowupRequired = dto.FollowupRequired;
            dao.CaseStatus = dto.CaseStatus;
            dao.Resolution = dto.Resolution;
            dao.TriggeredFrom = dto.TriggeredFrom;
            dao.TriggeredBy = "AutoSave";
            return dao;
        }
    }
}
