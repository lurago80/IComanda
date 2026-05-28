using System;
using FirebirdSql.Data.FirebirdClient;
using Dapper;

var connectionString = "Server=localhost;Database=C:\\Users\\usuario\\Desktop\\Base\\Eduardo\\DADOSG5.FDB;User=SYSDBA;Password=masterkey;Port=3050;Charset=UTF8";

try
{
    using var connection = new FbConnection(connectionString);
    connection.Open();

    Console.WriteLine("\n=== USUARIOS E SENHAS ===");
    using (var cmd = connection.CreateCommand())
    {
        cmd.CommandText = "SELECT ID, NOME, SENHA, ATIVO FROM USUARIO ORDER BY ID";
        using var r = cmd.ExecuteReader();
        while (r.Read())
        {
            Console.WriteLine($"ID={r["ID"]} Nome={r["NOME"]?.ToString()?.Trim()} Senha={r["SENHA"]?.ToString()?.Trim()} Ativo={r["ATIVO"]}");
        }
    }

    Console.WriteLine("\n=== TESTE 2: Mapeamento Dapper (string? Cancelar) ===");
    var usuarios = connection.Query<UsuarioTest>(
        "SELECT FIRST 5 ID, NOME, ATIVO, VISUALIZAR, TOTAL, TIPO, CANCELAR FROM USUARIO WHERE ATIVO='1'");
    foreach (var u in usuarios)
    {
        Console.WriteLine($"ID={u.Id} Nome={u.Nome?.Trim()}");
        Console.WriteLine($"  Cancelar='{u.Cancelar ?? "NULL"}' | Visualizar='{u.Visualizar ?? "NULL"}'");
        var podeCancel = u.Cancelar?.Equals("1", StringComparison.OrdinalIgnoreCase) ?? false;
        var podeViz = u.Visualizar?.Equals("1", StringComparison.OrdinalIgnoreCase) ?? false;
        var role = podeCancel ? "Gerente" : podeViz ? "Caixa" : "Garcom";
        Console.WriteLine($"  => Role: {role}");
    }
    Console.WriteLine("\nFim do teste.");
}
catch (Exception ex)
{
    Console.WriteLine($"ERRO: {ex.Message}");
}

// Classe precisa vir DEPOIS das top-level statements em C#
class UsuarioTest
{
    public int Id { get; set; }
    public string? Nome { get; set; }
    public string? Ativo { get; set; }
    public string? Visualizar { get; set; }
    public string? Total { get; set; }
    public string? Tipo { get; set; }
    public string? Cancelar { get; set; }
}

