using Microsoft.AspNetCore.Http;

namespace ARCServer.Business.Dtos.Products
{
    public class ProductSpinFormRequest
    {
        /// <summary>
        /// JSON array of frame keys in display order.
        /// Existing frame: "e:{id}". New file: "n:{index}" matching Images.
        /// </summary>
        public string FrameKeys { get; set; } = "[]";

        public List<IFormFile> Images { get; set; } = [];
    }
}
