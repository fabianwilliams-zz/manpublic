using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
using System.Threading.Tasks;
using fpg.common.DataObjects;
using fpg.common.DataServices;
using Microsoft.WindowsAzure.Storage.Table.DataServices;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage;
using System.IO;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using System.Diagnostics;
using Microsoft.WindowsAzure;
using System.Configuration;
using Newtonsoft.Json;
using fpg.common;

namespace fpg.web.Controllers
{
    public class PgItemController : Controller
    {

        private CloudQueue thumbnailRequestQueue;
        private static CloudBlobContainer imagesBlobContainer;

        public PgItemController()
        {
            InitializeStorage();
        }


        private void InitializeStorage()
        {
            // Open storage account using credentials from .cscfg file.
            var storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureWebJobsStorage"].ToString());

            // Get context object for working with blobs, and 
            // set a default retry policy appropriate for a web user interface.
            var blobClient = storageAccount.CreateCloudBlobClient();
            //blobClient.DefaultRequestOptions.RetryPolicy = new LinearRetry(TimeSpan.FromSeconds(3), 3);

            // Get a reference to the blob container.
            imagesBlobContainer = blobClient.GetContainerReference("images");

            // Get context object for working with queues, and 
            // set a default retry policy appropriate for a web user interface.
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            //queueClient.DefaultRequestOptions.RetryPolicy = new LinearRetry(TimeSpan.FromSeconds(3), 3);

            // Get a reference to the queue.
            thumbnailRequestQueue = queueClient.GetQueueReference("thumbnailrequest");
        }

        // GET: PgItem
        public ActionResult Index()
        {
            var items = DocumentDBRepository<PgItem>.GetAllItems();
            return View(items);
        }

        public ActionResult Details(string id)
        {
            var item = DocumentDBRepository<PgItem>.GetItem(d => d.Id == id);
            return View(item);

        }

        public ActionResult Delete(string id)
        {
            //string itemToDelete = (d => d.Id == id);
            var item = DocumentDBRepository<PgItem>.DeleteItemAsync(id);
            return View();

        }

        //Create Item
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(
            [Bind(Include = "id,PlaygroundName,PlaygroundAddress,PlaygroundGeoLonLat,Amenities,Added,Visit")]PgItem item,
            HttpPostedFileBase imageFile)
        {
            CloudBlockBlob imageBlob = null;

            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.ContentLength != 0)
                {
                    imageBlob = await UploadAndSaveBlobAsync(imageFile);
                    item.PlaygroundMainImage = imageBlob.Uri.ToString();
                    item.Id = Guid.NewGuid().ToString();

                    /*
                    //add a dummy amenity
                    item.Amenities[0].Name = "TBD";
                    item.Amenities[0].Amount = "TBD";
                    item.Amenities[0].Pictures[0].Image = "TBD";

                    //add a dummy visit
                    item.Visit[0].On = DateTime.Now.ToString();
                    item.Visit[0].Ratings = "3";
                    item.Visit[0].Comments = "TBD";
                    item.Visit[0].Person.FirstName = "TBD";
                    item.Visit[0].Person.LastName = "TBD";
                    item.Visit[0].Person.Origin[0].Idp = "TBD";
                    item.Visit[0].Person.Origin[0].UUid = "TBD";
                    item.Visit[0].Person.Origin[0].Email = "TBD";
                    */
                }




                Trace.TraceInformation("Updated PGItemID {0} in DocumentDB Database", item.Id);


                await DocumentDBRepository<PgItem>.CreateItemAsync(item);

                if (imageBlob != null)
                {
                    BlobInformation blobInfo = new BlobInformation() { PGItemId = item.Id, BlobUri = new Uri(item.PlaygroundMainImage) };
                    var queueMessage = new CloudQueueMessage(JsonConvert.SerializeObject(blobInfo));
                    await thumbnailRequestQueue.AddMessageAsync(queueMessage);
                    Trace.TraceInformation("Created queue message for PGItemId {0}", item.Id);
                }

                return RedirectToAction("Index");
            }
            return View(item);
        }

        //Edit Item
        public ActionResult Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            PgItem item = (PgItem)DocumentDBRepository<PgItem>.GetItem(d => d.Id == id);

            if (item == null)
            {
                return HttpNotFound();
            }

            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "id,PlaygroundName,PlaygroundMainImage,PlaygroundMainImageThumb,PlaygroundAddress,PlaygroundGeoLonLat,Amenities,Added,Visit")] PgItem item)
        {
            if (ModelState.IsValid)
            {
                await DocumentDBRepository<PgItem>.UpdateItemAsync(item.Id, item);
                return RedirectToAction("Index");
            }

            return View(item);
        }

        public ActionResult AddAmenities(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            PgItem item = (PgItem)DocumentDBRepository<PgItem>.GetItem(d => d.Id == id);

            if (item == null)
            {
                return HttpNotFound();
            }

            return View(item);

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddAmenities(
             [Bind(Include = "id,PlaygroundName,PlaygroundMainImage,PlaygroundMainImageThumb,PlaygroundAddress,PlaygroundGeoLonLat,Amenities,Added,Visit")]PgItem item,
            HttpPostedFileBase imageFile)
        {

            if (ModelState.IsValid)
            {

                await DocumentDBRepository<PgItem>.UpdateItemAsync(item.Id, item);
                return RedirectToAction("Index");
            }

            return View(item);
        }

        public ActionResult AddVisits(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            PgItem item = (PgItem)DocumentDBRepository<PgItem>.GetItem(d => d.Id == id);

            if (item == null)
            {
                return HttpNotFound();
            }

            return View(item);

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddVisits(
             [Bind(Include = "id,PlaygroundName,PlaygroundMainImage,PlaygroundMainImageThumb,PlaygroundAddress,PlaygroundGeoLonLat,Amenities,Added,Visit")]PgItem item)
        {

            if (ModelState.IsValid)
            {

                //await DocumentDBRepository<PgItem>.UpdateItemAsync(item.Id, item);
                await DocumentDBRepository<PgItem>.AddVisit(item.Id, item);
                return RedirectToAction("Index");
            }

            return RedirectToAction("Index");
        }

        private async Task<CloudBlockBlob> UploadAndSaveBlobAsync(HttpPostedFileBase imageFile)
        {
            Trace.TraceInformation("Uploading image file {0}", imageFile.FileName);

            string blobName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
            // Retrieve reference to a blob. 
            CloudBlockBlob imageBlob = imagesBlobContainer.GetBlockBlobReference(blobName);
            // Create the blob by uploading a local file.
            using (var fileStream = imageFile.InputStream)
            {
                await imageBlob.UploadFromStreamAsync(fileStream);
            }

            Trace.TraceInformation("Uploaded image file to {0}", imageBlob.Uri.ToString());

            return imageBlob;
        }
    }
}