using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Ropa_Segunda_Tienda_Virtual.Models;

namespace Ropa_Segunda_Tienda_Virtual.Controllers
{
    public class PrendaController : ApiController
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["RopaDb"].ConnectionString;

        [HttpPost]
        [Route("api/prenda")]
        public IHttpActionResult AgregarPrendaYCliente([FromBody] Prenda prenda)
        {
            try
            {
                using (var con = new SqlConnection(connectionString))
                {
                    con.Open();

                    // Verificar si el cliente ya existe
                    var cmdCliente = new SqlCommand("SELECT Documento FROM Cliente WHERE Email = @correo", con);
                    cmdCliente.Parameters.AddWithValue("@correo", prenda.Cliente1.Email);
                    var result = cmdCliente.ExecuteScalar();

                    if (result == null)
                    {
                        // Insertar cliente
                        var cmdInsertCliente = new SqlCommand("INSERT INTO Cliente (Documento, Nombre, Email, Celular) VALUES (@doc, @nom, @email, @cel)", con);
                        cmdInsertCliente.Parameters.AddWithValue("@doc", prenda.Cliente1.Documento);
                        cmdInsertCliente.Parameters.AddWithValue("@nom", prenda.Cliente1.Nombre);
                        cmdInsertCliente.Parameters.AddWithValue("@email", prenda.Cliente1.Email);
                        cmdInsertCliente.Parameters.AddWithValue("@cel", prenda.Cliente1.Celular);
                        cmdInsertCliente.ExecuteNonQuery();
                    }

                    // Insertar prenda
                    var cmdInsertPrenda = new SqlCommand("INSERT INTO Prenda (IdPrenda, TipoPrenda, Descripcion, Valor, Cliente) VALUES (@id, @tipo, @desc, @val, @cliente)", con);
                    cmdInsertPrenda.Parameters.AddWithValue("@id", prenda.IdPrenda);
                    cmdInsertPrenda.Parameters.AddWithValue("@tipo", prenda.TipoPrenda);
                    cmdInsertPrenda.Parameters.AddWithValue("@desc", prenda.Descripcion);
                    cmdInsertPrenda.Parameters.AddWithValue("@val", prenda.Valor);
                    cmdInsertPrenda.Parameters.AddWithValue("@cliente", prenda.Cliente1.Documento);
                    cmdInsertPrenda.ExecuteNonQuery();

                    return Ok(new { mensaje = "Cliente y prenda registrados correctamente", idPrenda = prenda.IdPrenda });
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}
