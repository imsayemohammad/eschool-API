using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using ESCHOOL.Common;
using Dapper;
using ESCHOOL.IServices;
using ESCHOOL.Models;
using Microsoft.Extensions.Configuration;

namespace ESCHOOL.Services
{
    public class ExamResultServices : IExamResultServices
    {
        //ExamResult _ExamResult = new ExamResult(); //from Model
        List<ExamResult> _ExamResultList = new List<ExamResult>(); //for resultset

        private readonly IConfiguration _configuration;

        public ExamResultServices(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public List<ExamResult> Gets()
        {
            _ExamResultList = new List<ExamResult>();

            using (IDbConnection con = new SqlConnection(Global.ConnectionsString))
            {
                if (con.State == ConnectionState.Closed) con.Open();
                var oStudents = con.Query<ExamResult>(@"SELECT * FROM ExamResult ").ToList();

                if (oStudents != null && oStudents.Count() > 0)
                {
                    _ExamResultList = oStudents;
                }
                con.Close();
            }
            return _ExamResultList;
        }


        List<ExamResult> IExamResultServices.Get(int pkId)
        {
            _ExamResultList = new List<ExamResult>();

            using (IDbConnection con = new SqlConnection(Global.ConnectionsString))
            {
                if (con.State == ConnectionState.Closed) con.Open();
                var oStudents = con.Query<ExamResult>(@"SELECT * FROM ExamResult WHERE ClassId='" + pkId + "' ").ToList();

                if (oStudents != null && oStudents.Count() > 0)
                {
                    _ExamResultList = oStudents;
                }
                con.Close();
            }
            return _ExamResultList;
        }

        //public List<ExamResult> GetExamResult()
        //{
        //    string connectionString = _configuration.GetConnectionString("StudentDB");

        //    _ExamResultList = new List<ExamResult>();

        //    using (IDbConnection con = new SqlConnection(connectionString))
        //    {
        //        if (con.State == ConnectionState.Closed) con.Open();
        //        var oStudents = con.Query<ExamResult>(@"SELECT [ED].[ExamTitle] AS SubjectName,
        //             JSON_QUERY(( 
        //              SELECT  [ExamType], [ExamDate], [ResultDate], [ClassID], [SectionID], [SubjectId], [FullMarks], [GetMarks]
        //              FROM [dbo].[ExamResult] [ER], [dbo].[ExamSetup] [ES]
        //              WHERE [ER].[ExamDetailId] = [ED].[ExamDetailId] AND [ED].ExamTypeId = [ES].ExamSetupID
        //              FOR JSON PATH 
        //              )) AS [ExamResults]
        //            FROM [dbo].[ExamDetails] [ED]
        //            FOR 
        //             JSON PATH, WITHOUT_ARRAY_WRAPPER").ToList();

        //        if (oStudents != null && oStudents.Count() > 0)
        //        {
        //            _ExamResultList = oStudents;
        //        }
        //        con.Close();
        //    }
        //    return _ExamResultList;
        //}


        public string GetExamResult(int id)
        {
            string connectionString = _configuration.GetConnectionString("StudentDB");

            var oStudents = "";

            using (IDbConnection con = new SqlConnection(connectionString))
            {
                if (con.State == ConnectionState.Closed) con.Open();
                oStudents = con.Query<string>(@"SELECT [S].[SubjectName] AS [SubjectName],
	                                    JSON_QUERY(( 
		                                    SELECT  [ExamType], [ExamDate], [ResultDate], [ER].[ClassID], [SectionID], [SubjectId], [SubjectName], [FullMarks], [GetMarks], [Remarks]
		                                    FROM [dbo].[ExamResult] [ER], [dbo].[ExamSetup] [ES], [dbo].[Subjects] [S]
		                                    WHERE [ER].[ExamDetailId] = [ED].[ExamDetailId] AND [ED].ExamTypeId = [ES].ExamSetupID 
		                                    AND [ER].ClassId = [ED].ClassID AND [ER].ClassId = [S].ClassID
		                                    AND [ER].SectionId = [ED].SectionId AND [ER].SchoolId = [ED].SchoolId
		                                    AND [ED].SubjectId = [S].SubjecId AND [ER].[StudentId] = '" + id + "' " +
                                            "FOR JSON PATH )) AS [ResultDetails] " +
                                            "FROM [dbo].[ExamDetails] [ED] INNER JOIN [dbo].[ExamResult] [ER] ON [ER].[ExamDetailId] = [ED].[ExamDetailId] " +
                                            "INNER JOIN [dbo].[Subjects] [S] ON [ED].SubjectId = [S].SubjecId " +
                                            "WHERE [ER].[StudentId] = '" + id + "' " +
                                        "FOR JSON PATH ").FirstOrDefault();


                //oStudents = con.Query<string>(@"SELECT [ED].[ExamTitle] AS ExamTitle,
                //             JSON_QUERY(( 
                //              SELECT  [ExamType], [ExamDate], [ResultDate], [ER].[ClassID], [SectionID], [SubjectId], [SubjectName], [FullMarks], [GetMarks], [Remarks]
                //              FROM [dbo].[ExamResult] [ER], [dbo].[ExamSetup] [ES], [dbo].[Subjects] [S]
                //              WHERE [ER].[ExamDetailId] = [ED].[ExamDetailId] AND [ED].ExamTypeId = [ES].ExamSetupID 
                //              AND [ER].ClassId = [ED].ClassID AND [ER].ClassId = [S].ClassID
                //              AND [ER].SectionId = [ED].SectionId AND [ER].SchoolId = [ED].SchoolId
                //              AND [ED].SubjectId = [S].SubjecId
                //              FOR JSON PATH 
                //              )) AS [ExamResults]
                //                FROM [dbo].[ExamDetails] [ED] INNER JOIN [dbo].[ExamResult] [ER] ON [ER].[ExamDetailId] = [ED].[ExamDetailId] 
                //                WHERE [ER].[StudentId] = '" + id + "' " +
                //                "FOR JSON PATH ").FirstOrDefault();

                con.Close();
            }
            return oStudents;
        }

    }
}

