using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Business
{
    public class CustomerMedia
    {
        public CustomerMedia() { }
        public CustomerMedia(MediaType t) { }

        //
        // Resumen:
        //     For barcode media, returns data read during media processing or empty string
        //     if barcode was not read.
        public string BarcodeRead { get; set; }
        //
        // Resumen:
        //     For passbook media, returns track data read during media processing or empty
        //     string if track was not read.
        public string PassbookTrack { get; set; }
        //
        // Resumen:
        //     For card media, returns Track3 data read during media processing or empty string
        //     if track3 not read.
        public string CardTrack3 { get; set; }
        //
        // Resumen:
        //     For card media, returns Track2 data read during media processing or empty string
        //     if track2 not read.
        public string CardTrack2 { get; set; }
        //
        // Resumen:
        //     For card media, returns Track1 data read during media processing or empty string
        //     if track1 not read.
        public string CardTrack1 { get; set; }
        //
        // Resumen:
        //     Institution Id extracted from card/passbook Empty if no account number location
        //     specified in media processing rules.
        public string InstitutionId { get; set; }
        //
        // Resumen:
        //     AccountNumber extracted from card/passbook according to media processing rules.
        //     Empty if no account number location specified in media processing rules.
        public string AccountNumber { get; set; }
   
        public MediaType Type { get; set; }

        public enum MediaType
        {
            Card = 0,
            Passbook = 1,
            None = 2,
            Barcode = 3
        }
    }
}
