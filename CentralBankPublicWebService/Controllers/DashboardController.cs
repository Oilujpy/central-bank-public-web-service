using CentralBankPublicWebService.DTOs;
using CentralBankPublicWebService.Models;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;

namespace CentralBankPublicWebService.Controllers
{
    public class DashboardController : Controller
    {
        // GET: Dashboard
        public ActionResult Index()
        {
            List<PublicWebServiceHistoricalUseResult> publicWebServiceHistoricalUseResult = new List<PublicWebServiceHistoricalUseResult>();

            using (var connection = new MySqlConnection(ConfigurationManager.ConnectionStrings["default"].ConnectionString))
            {
                connection.Open();
                string query = "SELECT CONSULTA_SERVICIO.FECHA_INVOCACION, CONSULTA_SERVICIO.FECHA_FINALIZACION, CONSULTA_SERVICIO.IP_SOLICITANTE, SERVICIO.NOMBRE " +
                    "FROM CONSULTA_SERVICIO " +
                    "INNER JOIN SERVICIO ON CONSULTA_SERVICIO.SERVICIO_ID = SERVICIO.SERVICIO_ID";

                using (var command = new MySqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            publicWebServiceHistoricalUseResult.Add(new PublicWebServiceHistoricalUseResult
                            {
                                InvocationStart = reader.GetDateTime(0),
                                InvocationEnd = reader.GetDateTime(1),
                                RequestorIp = reader.GetString(2),
                                MethodName = reader.GetString(3),
                            });
                        }
                    }
                }
            }

            DashboardViewModel dashboardViewModel = new DashboardViewModel();


            var casaCambio = publicWebServiceHistoricalUseResult.FindAll(x => x.MethodName == "CONSULTA TASA DE CAMBIO");

            var grouped = publicWebServiceHistoricalUseResult
                .GroupBy(c => new
                {
                    c.InvocationStart.Month,
                    c.InvocationStart.Year,
                    c.MethodName,
                })
                .OrderBy(x => x.Key.Year).ThenBy(x => x.Key.Month);

            foreach (var group in grouped)
            {
                var cap = group.Count();
                dashboardViewModel.BarChartData.Add(new BarChartData
                {
                    Caption = group.Key.MethodName,
                    Value = group.Count().ToString(),
                    Label = $"{group.Key.Year}-{group.Key.Month}"
                });
            }

            return View(dashboardViewModel);
        }
    }
}