using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Pescheria.Models;
using Microsoft.Data.SqlClient;


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
            // eliminare l'immagine vecchia
            //if (System.IO.File.Exists(fullPath))
            //{
            //    System.IO.File.Delete(fullPath);
            //}

            // salviamo il file che ci è stato inviato
            string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            string fileName = Path.GetFileName(image.FileName);
            string fullFilePath = Path.Combine(path, fileName);
            //string fullFilePath = $"{Directory.GetCurrentDirectory()}\\wwwroot\\uploads\\{Path.GetFileName(image.FileName)}";
            FileStream stream = new FileStream(fullFilePath, FileMode.Create);
            image.CopyTo(stream);


            // validare i dati
            var newFish = StaticDb.Add(fish.Name, fish.IsSeaFish, fish.Price * 100);
            //return Redirect("https://localhost:7029/fish/details/" + fish.FishId);
            return RedirectToAction("Details", new { id = newFish.FishId });
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
            var fish = StaticDb.GetById(id);
            return View(fish);
        }

        [HttpPost]
        public IActionResult DeletePost(Fish fish)
        {
            var fishDeleted = StaticDb.HardDelete(fish.FishId);
            // qui so che la risorsa è stata eliminata e quale
            if (fishDeleted is not null)
            {
                TempData["MessageSuccess"] = $"Il pesce {fishDeleted.Name} è stato eliminato";
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
    }
}



