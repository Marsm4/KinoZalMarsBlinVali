using KinoZalMarsBlinVali.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinoZalMarsBlinVali.ViewModel
{
    public class SeatViewModel
    {
        public HallSeat Seat { get; set; }
        public string DisplayText => $"{Seat.RowNumber}-{Seat.SeatNumber}";
        public string SeatType => Seat.SeatType;

        public SeatViewModel(HallSeat seat)
        {
            Seat = seat;
        }
    }
}
