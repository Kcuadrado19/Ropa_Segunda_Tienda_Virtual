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

        [RoutePrefix("api/consulta")]
        public class ConsultaController : ApiController
        {
            private string connectionString = ConfigurationManager.ConnectionStrings["RopaDb"].ConnectionString;

            [HttpGet]
            [Route("{email}")]
            public IHttpActionResult GetPrendasCliente(string email)
            {
                var resultado = new
                {
                    Cliente = new Cliente(),
                    Prendas = new List<object>()
                };

                using (var con = new SqlConnection(connectionString))
                {
                    con.Open();

                    // 1. Obtener cliente por Email
                    var cmdCliente = new SqlCommand("SELECT * FROM Cliente WHERE Email = @correo", con);
                    cmdCliente.Parameters.AddWithValue("@correo", email);
                    var readerCliente = cmdCliente.ExecuteReader();

                    if (!readerCliente.Read())
                    {
                        return NotFound(); // Cliente no encontrado
                    }

                    resultado.Cliente.Documento = readerCliente["Documento"].ToString();
                    resultado.Cliente.Nombre = readerCliente["Nombre"].ToString();
                    resultado.Cliente.Email = readerCliente["Email"].ToString();
                    resultado.Cliente.Celular = readerCliente["Celular"].ToString();
                    string documento = resultado.Cliente.Documento;
                    readerCliente.Close();

                    // 2. Obtener prendas del cliente
                    var cmdPrendas = new SqlCommand("SELECT * FROM Prenda WHERE Cliente = @doc", con);
                    cmdPrendas.Parameters.AddWithValue("@doc", documento);
                    var readerPrendas = cmdPrendas.ExecuteReader();

                    var prendas = new List<dynamic>();
                    while (readerPrendas.Read())
                    {
                        prendas.Add(new
                        {
                            IdPrenda = (int)readerPrendas["IdPrenda"],
                            TipoPrenda = readerPrendas["TipoPrenda"].ToString(),
                            Descripcion = readerPrendas["Descripcion"].ToString(),
                            Valor = (float)readerPrendas["Valor"],
                            Imagenes = new List<string>()
                        });
                    }
                    readerPrendas.Close();

                    // 3. Obtener imágenes por cada prenda
                    foreach (var prenda in prendas)
                    {
                        var cmdImg = new SqlCommand("SELECT FotoPrenda FROM FotoPrenda WHERE idPrenda = @idp", con);
                        cmdImg.Parameters.AddWithValue("@idp", prenda.IdPrenda);
                        var readerImg = cmdImg.ExecuteReader();

                        var imagenes = new List<string>();
                        while (readerImg.Read())
                        {
                            imagenes.Add(readerImg["FotoPrenda"].ToString());
                        }
                        readerImg.Close();

                        resultado.Prendas.Add(new
                        {
                            prenda.IdPrenda,
                            prenda.TipoPrenda,
                            prenda.Descripcion,
                            prenda.Valor,
                            Imagenes = imagenes
                        });
                    }
                }

                return Ok(resultado);
            }
        }
    }
