using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Pescheria.Models;
using Microsoft.Data.SqlClient;
using static System.Net.Mime.MediaTypeNames;


namespace Pescheria.Controllers
{
    public class FishController : Controller
    {
        private string connString = "Server=LAPTOP-1M2QKVCO\\SQLEXPRESS; Initial Catalog=Pescheria; Integrated Security=true; TrustServerCertificate=True";
        // lista dei pesci
        [HttpGet]
        public IActionResult Index()
        {
            // aprire la connessione
            var conn = new SqlConnection(connString);
            List<Fish> fishes = [];

            try
            {
                conn.Open();
                // creare il comando
                var command = new SqlCommand("select * from Fish", conn);

                // eseguire il comando
                var reader = command.ExecuteReader();

                // usare i dati
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        var fish = new Fish()
                        {
                            FishId = (int)reader["FishId"],
                            Name = reader["Name"].ToString(),
                            IsSeaFish = (bool)reader["IsSeaFish"],
                            Price = (int)reader["Price"],
                            // DeletedAt = (DateTime)reader["DeletedAt"], // TODO: check
                            Image = reader["Image"].ToString()
                        };
                        fishes.Add(fish);
                    }
                }
            }
            catch (Exception ex)
            {
                return View("Error");
            }

            return View(fishes);
        }

        // pagina di dettaglio di un singolo pesce
        [HttpGet]
        public IActionResult Details([FromRoute] int? id)
        {
            var error = true;
            var fish = new Fish();

            if (id.HasValue)  // (id is null)
            {
                // aprire la connessione
                var conn = new SqlConnection(connString);
                try
                {
                    conn.Open();
                    // creare il comando
                    var command = new SqlCommand("select * from Fish where FishId=@fishId", conn);
                    command.Parameters.AddWithValue("@fishId", id);

                    // eseguire il comando
                    var reader = command.ExecuteReader();

                    // usare i dati
                    if (reader.HasRows)
                    {
                        reader.Read();
                        fish.FishId = (int)reader["FishId"];
                        fish.Name = reader["Name"].ToString();
                        fish.IsSeaFish = (bool)reader["IsSeaFish"];
                        fish.Price = (int)reader["Price"];
                        // fish.DeletedAt = (DateTime)reader["DeletedAt"]; // TODO: check
                        fish.Image = reader["Image"].ToString();
                        error = false;
                    }
                }
                catch (Exception ex) {}
                finally { conn.Close(); }
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }

            if (error) { return View("Error"); } else { return View(fish); }
        }

        // pagina con un form per l'aggiunta di un nuovo pesce
        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        // indirizzo per gestire il submit del form della pagina Add
        [HttpPost]
        public IActionResult Add(Fish fish, IFormFile image)
        {
            var error = true;
            var conn = new SqlConnection(connString);
            try
            {
                // validare i dati

                conn.Open();

                // salviamo il file che ci è stato inviato
                string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                string fileName = Path.GetFileName(image.FileName);
                string fullFilePath = Path.Combine(path, fileName);
                // TODO: generare un nome univoco
                //string fullFilePath = $"{Directory.GetCurrentDirectory()}\\wwwroot\\uploads\\{Path.GetFileName(image.FileName)}";
                FileStream stream = new FileStream(fullFilePath, FileMode.Create);
                image.CopyTo(stream);

                // creare il comando
                var command = new SqlCommand(@"
                    INSERT INTO Fish
                    (Name, IsSeaFish, Price, Image) VALUES
                    (@name, @isSeaFish, @price, @image)", conn);
                command.Parameters.AddWithValue("@name", fish.Name);
                command.Parameters.AddWithValue("@isSeaFish", fish.IsSeaFish);
                command.Parameters.AddWithValue("@price", fish.Price);
                // questo valore lo ricaviamo dopo aver salvato l'immagine nel disco
                command.Parameters.AddWithValue("@image", fileName);

                // eseguire il comando
                var nRows = command.ExecuteNonQuery();
                error = false;
            }
            catch (Exception ex) {}
            finally
            {
                conn.Close();
            }

            if (error) return View("Error"); else return RedirectToAction("Index");
            // l'ExecuteNonQuery non ritorna la risorsa creata ma solo il numero di righe
            //return RedirectToAction("Details", new { id = newFish.FishId });
        }

        // pagina con un form per la modififica di un pesce
        [HttpGet]
        public IActionResult Edit([FromRoute] int? id)
        {
            if (id is null) return RedirectToAction("Index", "Fish");

            var fish = StaticDb.GetById(id);
            if (fish is null) return View("Error");

            return View(fish);
        }

        [HttpPost]
        public IActionResult Edit(Fish fish)
        {
            var updatedFish = StaticDb.Modify(fish);
            if (updatedFish is null) return View("Error");

            return RedirectToAction("Details", new { id = updatedFish.FishId });
        }

        // rotta (indirizzo) per poter elimanare un pesce
        [HttpGet]
        public IActionResult Delete(int? id)
        {
            if (id is not null)
            {
                var fish = getById((int)id);
                return View(fish);
            }
            return RedirectToAction("Index");
            
        }

        [HttpPost]
        public IActionResult DeletePost(Fish fish)
        {
            var error = true;
            var deletedFish = new Fish();
            var conn = new SqlConnection(connString);
            try
            {
                // oppure fare con i parametri di output
                conn.Open();
                // creare il comando
                var commandRead = new SqlCommand("select * from Fish where FishId=@fishId", conn);
                commandRead.Parameters.AddWithValue("@fishId", fish.FishId);

                // eseguire il comando
                var reader = commandRead.ExecuteReader();

                // usare i dati
                if (reader.HasRows)
                {
                    reader.Read();
                    deletedFish.FishId = (int)reader["FishId"];
                    deletedFish.Name = reader["Name"].ToString();
                    deletedFish.IsSeaFish = (bool)reader["IsSeaFish"];
                    deletedFish.Price = (int)reader["Price"];
                    // deletedFish.DeletedAt = (DateTime)reader["DeletedAt"]; // TODO: check
                    deletedFish.Image = reader["Image"].ToString();
                }

                reader.Close();

                // creare il comando
                var commandDelete = new SqlCommand("DELETE FROM Fish WHERE  FishId=@fishId", conn);
                commandDelete.Parameters.AddWithValue("@fishId", fish.FishId);

                // eseguire il comando
                var nRows = commandDelete.ExecuteNonQuery();
                if (nRows > 0) {
                    error = false;

                    // eliminare anche l'immagine
                    if (deletedFish.Image is not null)
                    {
                        string fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", deletedFish.Image);
                        if (System.IO.File.Exists(fullPath))
                        {
                            System.IO.File.Delete(fullPath);
                        }
                    }
                }
            }
            catch (Exception ex) { }
            finally
            {
                conn.Close();
            }

            if (!error)
            {
                TempData["MessageSuccess"] = $"Il pesce {deletedFish.Name} è stato eliminato";
                return RedirectToAction("Index");
            }

            TempData["MessageError"] = "C'è stato un problema durante l'eliminazione";
            return RedirectToAction("Index");


        }

        [HttpPost]
        public IActionResult SoftDelete(Fish fish)
        {
            var fishDeleted = StaticDb.SoftDelete(fish.FishId);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Cestino()
        {
            var fishesDeleted = StaticDb.GetAllDeleted();
            return View(fishesDeleted);
        }

        [HttpPost]
        public IActionResult Recover(Fish fish)
        {
            var recoveredFish = StaticDb.Recover(fish.FishId);
            if (recoveredFish is null)
            {
                return RedirectToAction("Cestino");
            }
            return RedirectToAction("Details", new {id = recoveredFish.FishId });
        }

        /*********************/

        public IActionResult GetFile()
        {
            return PhysicalFile(Directory.GetCurrentDirectory() + "\\Contents\\spedire.txt", "text/plain");
        }

        public IActionResult GetSlug()
        {
            return Json("questo-slug-buono");
        }

        private Fish? getById(int id)
        {
            var error = true;
            var fish = new Fish();

            // aprire la connessione
            var conn = new SqlConnection(connString);
            try
            {
                conn.Open();
                // creare il comando
                var command = new SqlCommand("select * from Fish where FishId=@fishId", conn);
                command.Parameters.AddWithValue("@fishId", id);

                // eseguire il comando
                var reader = command.ExecuteReader();

                // usare i dati
                if (reader.HasRows)
                {
                    reader.Read();
                    fish.FishId = (int)reader["FishId"];
                    fish.Name = reader["Name"].ToString();
                    fish.IsSeaFish = (bool)reader["IsSeaFish"];
                    fish.Price = (int)reader["Price"];
                    // fish.DeletedAt = (DateTime)reader["DeletedAt"]; // TODO: check
                    fish.Image = reader["Image"].ToString();
                    error = false;
                }
            }
            catch (Exception ex) { }
            finally { conn.Close(); }

            if (error) return null; else return fish;
        }
    }
}



