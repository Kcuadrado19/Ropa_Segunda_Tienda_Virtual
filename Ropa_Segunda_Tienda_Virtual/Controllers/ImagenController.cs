using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Web;
using System.Web.Http;
using System.Collections.Generic;

namespace Ropa_Segunda_Tienda_Virtual.Controllers
{
    [RoutePrefix("api/imagen")]
    public class ImagenController : ApiController
    {
        // POST: api/imagen
        [HttpPost]
        [Route("")]
        public IHttpActionResult SubirImagen()
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;

                if (httpRequest.Files.Count == 0)
                    return BadRequest("No se envió ninguna imagen.");

                var postedFile = httpRequest.Files[0];
                var nombreArchivo = Path.GetFileName(postedFile.FileName);
                var rutaFisica = HttpContext.Current.Server.MapPath("~/Imagenes/");
                Directory.CreateDirectory(rutaFisica);
                var rutaCompleta = Path.Combine(rutaFisica, nombreArchivo);
                postedFile.SaveAs(rutaCompleta);

                var idPrenda = int.Parse(httpRequest.Form["idPrenda"]);
                var idFoto = new Random().Next(1000, 9999);

                using (var con = new SqlConnection(ConfigurationManager.ConnectionStrings["RopaDb"].ConnectionString))
                {
                    con.Open();
                    var cmd = new SqlCommand("INSERT INTO FotoPrenda (idFoto, FotoPrenda, idPrenda) VALUES (@idFoto, @foto, @idPrenda)", con);
                    cmd.Parameters.AddWithValue("@idFoto", idFoto);
                    cmd.Parameters.AddWithValue("@foto", nombreArchivo);
                    cmd.Parameters.AddWithValue("@idPrenda", idPrenda);
                    cmd.ExecuteNonQuery();
                }

                return Ok(new
                {
                    mensaje = "Imagen subida correctamente",
                    archivo = nombreArchivo,
                    url = $"/Imagenes/{nombreArchivo}"
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // PUT: api/imagen/{id}
        [HttpPut]
        [Route("{id}")]
        public IHttpActionResult ActualizarImagen(int id)
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;
                if (httpRequest.Files.Count == 0)
                    return BadRequest("No se envió ninguna imagen.");

                var postedFile = httpRequest.Files[0];
                var nuevoNombre = Path.GetFileName(postedFile.FileName);
                var rutaFisica = HttpContext.Current.Server.MapPath("~/Imagenes/");
                Directory.CreateDirectory(rutaFisica);
                var nuevaRuta = Path.Combine(rutaFisica, nuevoNombre);
                postedFile.SaveAs(nuevaRuta);

                using (var con = new SqlConnection(ConfigurationManager.ConnectionStrings["RopaDb"].ConnectionString))
                {
                    con.Open();

                    var getCmd = new SqlCommand("SELECT FotoPrenda FROM FotoPrenda WHERE idFoto = @id", con);
                    getCmd.Parameters.AddWithValue("@id", id);
                    var nombreViejo = getCmd.ExecuteScalar()?.ToString();

                    var cmd = new SqlCommand("UPDATE FotoPrenda SET FotoPrenda = @nuevoNombre WHERE idFoto = @id", con);
                    cmd.Parameters.AddWithValue("@nuevoNombre", nuevoNombre);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();

                    if (!string.IsNullOrEmpty(nombreViejo))
                    {
                        var rutaVieja = Path.Combine(rutaFisica, nombreViejo);
                        if (File.Exists(rutaVieja)) File.Delete(rutaVieja);
                    }
                }

                return Ok("Imagen actualizada correctamente.");
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // DELETE: api/imagen/{id}
        [HttpDelete]
        [Route("{id}")]
        public IHttpActionResult EliminarImagen(int id)
        {
            try
            {
                string nombreArchivo = null;
                using (var con = new SqlConnection(ConfigurationManager.ConnectionStrings["RopaDb"].ConnectionString))
                {
                    con.Open();

                    var getCmd = new SqlCommand("SELECT FotoPrenda FROM FotoPrenda WHERE idFoto = @id", con);
                    getCmd.Parameters.AddWithValue("@id", id);
                    nombreArchivo = getCmd.ExecuteScalar()?.ToString();

                    var delCmd = new SqlCommand("DELETE FROM FotoPrenda WHERE idFoto = @id", con);
                    delCmd.Parameters.AddWithValue("@id", id);
                    delCmd.ExecuteNonQuery();
                }

                if (!string.IsNullOrEmpty(nombreArchivo))
                {
                    var rutaFisica = HttpContext.Current.Server.MapPath("~/Imagenes/" + nombreArchivo);
                    if (File.Exists(rutaFisica))
                        File.Delete(rutaFisica);
                }

                return Ok("Imagen eliminada correctamente.");
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // GET: api/imagen/cliente/{documento}
        [HttpGet]
        [Route("cliente/{documento}")]
        public IHttpActionResult ObtenerImagenesPorCliente(string documento)
        {
            try
            {
                var lista = new List<object>();

                using (var con = new SqlConnection(ConfigurationManager.ConnectionStrings["RopaDb"].ConnectionString))
                {
                    con.Open();
                    var cmd = new SqlCommand(@"
                        SELECT c.Documento, c.Nombre, c.Email, c.Celular,
                               p.IdPrenda, p.TipoPrenda, p.Descripcion, p.Valor,
                               f.idFoto, f.FotoPrenda
                        FROM Cliente c
                        JOIN Prenda p ON c.Documento = p.Cliente
                        JOIN FotoPrenda f ON p.IdPrenda = f.idPrenda
                        WHERE c.Documento = @doc", con);

                    cmd.Parameters.AddWithValue("@doc", documento);
                    var reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        lista.Add(new
                        {
                            Cliente = new
                            {
                                Documento = reader["Documento"].ToString(),
                                Nombre = reader["Nombre"].ToString(),
                                Email = reader["Email"].ToString(),
                                Celular = reader["Celular"].ToString()
                            },
                            Prenda = new
                            {
                                IdPrenda = (int)reader["IdPrenda"],
                                TipoPrenda = reader["TipoPrenda"].ToString(),
                                Descripcion = reader["Descripcion"].ToString(),
                                Valor = (float)(double)reader["Valor"]
                            },
                            Foto = new
                            {
                                idFoto = (int)reader["idFoto"],
                                Nombre = reader["FotoPrenda"].ToString(),
                                Url = $"/Imagenes/{reader["FotoPrenda"]}"
                            }
                        });
                    }
                }

                return Ok(lista);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}
