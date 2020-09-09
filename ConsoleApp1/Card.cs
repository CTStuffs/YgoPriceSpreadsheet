using System;
using System.Collections.Generic;
using System.Text;

namespace YugiohPriceSpreadsheet
{
    public class Card
    {
        public string Name { get; set; }
        public int Quantity { get; set; }
        public string Rarity { get; set; }
        public string PackCode { get; set; }
        public double Price { get; set; }

        public Card(string name, int quantity, string rarity, string packcode, double price){
            this.Name = name;
            this.Quantity = quantity;
            this.Rarity = rarity;
            this.PackCode = packcode;
            this.Price = price;
        }

    }
}
