using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsianApi.Api.Model
{
    class Game
    {
        // команда на выезде
        public Team AwayTeam { get; set; }
        // команда дома
        public Team HomeTeam { get; set; }
        // Длина игры в минутах
        public int ExpectedLength { get; set; }
        // фаворит 
        public int Favoured { get; set; } // где применяется?
        // В игре минут
        public int InGameinutes { get; set; }
        // 1, если матч в прямом эфире, 0, если нет
        public int isLive { get; set; }
        // Вид сделки в тексте. Значения "живой", "Сегодня" или "Раннее"
        public string MarketType { get; set; }
        // Идентификатор рынка. Это 0 для Live, 1 сегодня и 2 для рано.
        public int MarketTypeId { get; set; }
        // Время_запуска или Kickoff время в миллисекундах с момента последнего Эпохи
        public ulong StartTime { get; set; }
        // Конверсированный строка даты / времени в StartTime
        public string StartsOn { get; set; }
        // Тип спорта
        public string SportsType { get; set; }
        // ид лиги
        public long LeagueId { get; set; }
        // Название лиги
        public string LeagueName { get; set; }
        // ид матча
        public long MatchId { get; set; }
        // Правда или ложь. Правда означает, что игра была удалена из платформы и, следовательно, потребитель службы должен удалить это из ответа
        public bool WillBeRemoved { get; set; }
        // Дата и время, когда игра будет удалена из этого канала в миллисекундах с прошлой эпохи. Если значение HasBeenRemoved ложно, то это -1
        public long ToBeRemovedOn { get; set; }
        // Правда или ложь. Правда означает, что вы можете сделать ставку на эту игру, ложь в противном случае.
        public bool isActive { get; set; }
        // Последний раз этот конкретный матч был обновлен в миллисекундах с момента последней эпохи
        public ulong UpdatedDateTime { get; set; }
        // Лист событий
        public List<EventModel> EventsList { get; set; }
    }
}
