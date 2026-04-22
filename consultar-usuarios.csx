using System;
using FirebirdSql.Data.FirebirdClient;

var connectionString = "Server=localhost;Database=C:\\iComanda\\Dados\\DADOSG5.FDB;User=SYSDBA;Password=masterkey;Port=3050;Charset=UTF8";

try
{
    using var connection = new FbConnection(connectionString);
    connection.Open();
    
    using var command = connection.CreateCommand();
    command.CommandText = "SELECT ID, NOME, SENHA, ATIVO FROM USUARIOS WHERE ATIVO='1' ORDER BY ID FETCH FIRST 10 ROWS ONLY";
    
    using var reader = command.ExecuteReader();
    
    Console.WriteLine("\n========================================");
    Console.WriteLine("Usuários Ativos no Banco de Dados");
    Console.WriteLine("========================================\n");
    
    while (reader.Read())
    {
        var id = reader["ID"];
        var nome = reader["NOME"]?.ToString()?.Trim();
        var senha = reader["SENHA"]?.ToString()?.Trim();
        var ativo = reader["ATIVO"]?.ToString()?.Trim();
        
        Console.WriteLine($"ID: {id}");
        Console.WriteLine($"Nome: {nome}");
        Console.WriteLine($"Senha: {senha}");
        Console.WriteLine($"Ativo: {ativo}");
        Console.WriteLine("----------------------------------------");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Erro: {ex.Message}");
    Console.WriteLine($"Stack: {ex.StackTrace}");
}
