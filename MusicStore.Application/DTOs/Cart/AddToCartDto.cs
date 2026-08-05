using System;
using System.Collections.Generic;
using System.Text;

namespace MusicStore.Application.DTOs.Cart
{
    public class AddToCartDto
    {
        public int ProductId { get; set; }

        public int Quantity { get; set; } = 1;
    }
}
