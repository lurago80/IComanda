namespace IComanda.API.Models.DTOs;

public class RelatorioDashboardDto
{
    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }

    // Resumo geral
    public int TotalVendas { get; set; }
    public decimal ValorTotal { get; set; }
    public decimal TicketMedio { get; set; }

    // Ticket médio por dia
    public List<TicketMedioDiaDto> TicketPorDia { get; set; } = [];

    // Vendas por hora do dia (0-23)
    public List<VendasPorHoraDto> VendasPorHora { get; set; } = [];

    // Vendas por dia da semana (0=Dom, 1=Seg ... 6=Sáb)
    public List<VendasPorDiaSemanaDto> VendasPorDiaSemana { get; set; } = [];
}

public class TicketMedioDiaDto
{
    public DateTime Data { get; set; }
    public int Quantidade { get; set; }
    public decimal ValorTotal { get; set; }
    public decimal TicketMedio { get; set; }
}

public class VendasPorHoraDto
{
    public int Hora { get; set; }
    public int Quantidade { get; set; }
    public decimal ValorTotal { get; set; }
}

public class VendasPorDiaSemanaDto
{
    public int DiaSemana { get; set; }
    public string NomeDia { get; set; } = "";
    public int Quantidade { get; set; }
    public decimal ValorTotal { get; set; }
}
