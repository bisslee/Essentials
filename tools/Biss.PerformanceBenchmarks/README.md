# Benchmarks de Performance - Biss Essentials

Este projeto contém benchmarks de performance para comparar os componentes Biss com alternativas como AutoMapper e MediatR.

## 🔧 Pré-requisitos

- .NET 9 SDK
- Visual Studio 2022 ou VS Code

## 🚀 Como Executar

### Executar todos os benchmarks
```bash
dotnet run --project tools/Biss.PerformanceBenchmarks -- --filter "*"
```

### Executar benchmarks específicos de Mediator
```bash
dotnet run --project tools/Biss.PerformanceBenchmarks -- --filter "MediatorBenchmarks*"
```

### Executar benchmarks específicos de Mapper
```bash
dotnet run --project tools/Biss.PerformanceBenchmarks -- --filter "MapperBenchmarks*"
```

## 📊 Benchmarks Disponíveis

### Mediator Benchmarks
- **SendCommand**: Mede a performance de envio de comandos
- **SendQuery**: Mede a performance de envio de queries
- **PublishNotification**: Mede a performance de publicação de notificações
- **SendCommandWithBehaviors**: Mede a performance com behaviors de pipeline

### Mapper Benchmarks
- **MapUser**: Mapeamento simples de objetos
- **MapUserWithConverter**: Mapeamento com conversor customizado

## 📈 Resultados Esperados

Os benchmarks fornecem métricas detalhadas incluindo:
- **Mean**: Tempo médio de execução
- **Error**: Margem de erro
- **StdDev**: Desvio padrão
- **Gen 0/1/2**: Coleta de lixo por geração
- **Allocated**: Memória alocada

## 📝 Exemplo de Saída

```
|                    Method |       Mean |     Error |    StdDev |  Gen 0 |  Gen 1 | Gen 2 | Allocated |
|-------------------------- |-----------|-----------|-----------|--------|--------|-------|-----------|
|            SendCommand |   234.7 μs |   2.34 μs |   2.09 μs | 0.0401 |      - |     - |     624 B |
|               SendQuery |   456.2 μs |   4.56 μs |   4.56 μs | 0.0802 |      - |     - |    1248 B |
|     PublishNotification |   123.4 μs |   1.23 μs |   1.09 μs | 0.0200 |      - |     - |     312 B |
```

## 🔍 Personalizar Benchmarks

Para adicionar novos benchmarks, crie uma nova classe que herde de `BenchmarkBase` e adicione métodos marcados com o atributo `[Benchmark]`.

## 📚 Documentação Adicional

Para mais informações sobre o projeto, consulte:
- [Documentação dos Componentes](../README.md)
- [Especificação Técnica](../Especificação%20Técnica%20Melhorada_%20Componentes%20Mediator%20e%20Mapper%20para%20.NET%209.md)

## 🛠️ Desenvolvimento

### Adicionar Novos Benchmarks

1. Crie uma nova classe de benchmark
2. Implemente métodos com `[Benchmark]`
3. Execute os benchmarks com `dotnet run`

### Exemplo de Benchmark

```csharp
[MemoryDiagnoser]
public class MeuBenchmark
{
    [GlobalSetup]
    public void Setup()
    {
        // Configuração inicial
    }

    [Benchmark]
    public void MeuMetodo()
    {
        // Código a ser medido
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        // Limpeza final
    }
}
```

## 📄 Licença

MIT License - Biss Essentials
