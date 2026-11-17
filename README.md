# Tô Bem — API de Telemetria (ASP.NET Core 8 + EF Core + Oracle)

API REST da disciplina **Advanced Business Development with .NET** (Global Solution 2026/2 · CitizenTech · FIAP).

## Integrantes

Lucas Shida Rodrigues da Costa RM568194
Matheus Guimarães Rosa RM567912
Pedro Henrique Pinheiro Falchi RM566967

- **Cidadão beneficiado:** idosos que moram sozinhos e suas famílias.

## O que o projeto resolve

O **Tô Bem** é uma lâmpada com radar que percebe a rotina de um idoso que mora sozinho, acende uma luz noturna para evitar quedas, detecta quando ele fica imóvel por tempo demais e avisa a família, sem câmera.

Esta API é a parte que **recebe os dados do dispositivo (ESP32)** e os guarda no Oracle: cada lâmpada/base cadastrada é um *Dispositivo* e cada leitura que ele envia (movimento, presença parada, botão, temperatura, heartbeat) é um *Evento*.

## Modelo de dados (relação 1:N)

```
DISPOSITIVO (1) ───────< (N) EVENTO
```

**DISPOSITIVO**

| Coluna | Tipo | Regra |
|--------|------|-------|
| ID | número (identity) | chave primária |
| IDOSO_ID | número | obrigatório, maior que 0 (referência ao idoso, tabela da API Java) |
| TIPO | texto(10) | `LAMPADA` ou `BASE` |
| COMODO | texto(50) | obrigatório, 2 a 50 caracteres |
| STATUS | texto(10) | `ONLINE` ou `OFFLINE` (padrão `OFFLINE`) |
| ULTIMO_HEARTBEAT | data/hora | preenchido automaticamente por eventos `HEARTBEAT` |

**EVENTO**

| Coluna | Tipo | Regra |
|--------|------|-------|
| ID | número (identity) | chave primária |
| DISPOSITIVO_ID | número | chave estrangeira para DISPOSITIVO (exclusão em cascata) |
| TIPO | texto(30) | um dos tipos válidos (abaixo) |
| VALOR | número | valor da leitura (ex.: 1 = presença, temperatura em °C) |
| DATA_HORA | data/hora | se omitida na requisição, usa a hora atual (UTC) |

Tipos de evento válidos: `MOVIMENTO`, `PRESENCA_PARADA`, `SEM_PRESENCA`, `GELADEIRA_ABERTA`, `BOTAO_OK`, `BOTAO_AUSENTE`, `BOTAO_VOLTEI`, `TEMPERATURA`, `HEARTBEAT`.

## Tecnologias

- ASP.NET Core 8 (Web API com controllers)
- Entity Framework Core 8.0.8 com provider Oracle (`Oracle.EntityFrameworkCore` 8.23.60 — mesmas versões do projeto de exemplo da aula)
- Migrations versionadas na pasta `Migrations/`
- Banco: Oracle da FIAP (`oracle.fiap.com.br`, serviço `orcl`)

## Como executar

### 1. Pré-requisitos

- [.NET SDK 8](https://dotnet.microsoft.com/download/dotnet/8.0) (ou Visual Studio 2022 com a carga de trabalho ASP.NET)
- Usuário e senha do Oracle da FIAP (o mesmo RM/senha usado nas aulas)
- Para usar o terminal: `dotnet tool install --global dotnet-ef --version 8.0.8`

### 2. Credenciais (nunca vão para o repositório)

A API **não** tem senha no código. Escolha **uma** das formas:

**Forma A — variáveis de ambiente**

PowerShell (Windows):

```powershell
$env:ORACLE_USER = "RM000000"
$env:ORACLE_PASSWORD = "sua_senha"
```

Bash (Linux/macOS):

```bash
export ORACLE_USER=RM000000
export ORACLE_PASSWORD=sua_senha
```

Host, porta e serviço já têm o padrão da FIAP (`oracle.fiap.com.br`, `1521`, `orcl`). Para mudar: `ORACLE_HOST`, `ORACLE_PORT`, `ORACLE_SERVICE`.

**Forma B — user-secrets (guarda fora da pasta do projeto)**

```bash
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "User Id=RM000000;Password=sua_senha;Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=oracle.fiap.com.br)(PORT=1521)))(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=orcl)))"
```

No Visual Studio: botão direito no projeto → *Gerenciar Segredos do Usuário* e cole o JSON `{ "ConnectionStrings": { "DefaultConnection": "..." } }`.

Também vale `ORACLE_CONNECTION_STRING` (string completa) como variável de ambiente.

### 3. Restaurar, criar as tabelas (Migrations) e rodar

Dentro da pasta do projeto:

```bash
dotnet restore
dotnet ef database update      # aplica a Migration InitialCreate: cria DISPOSITIVO e EVENTO no Oracle
dotnet run
```

No Visual Studio, em vez do `dotnet ef`, use o *Console do Gerenciador de Pacotes*: `Update-Database`.

A API sobe em `http://localhost:5000`. Teste rápido: abra `http://localhost:5000/` e veja
`{"api":"Tô Bem - API de Telemetria","status":"no ar"}`.

### Se precisar recriar as Migrations

Só faça isso se o `Update-Database` falhar por causa da Migration. Apague a pasta `Migrations/` e, se as tabelas já existirem no Oracle, apague-as (`DROP TABLE EVENTO; DROP TABLE DISPOSITIVO;`). Depois:

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

(Visual Studio: `Add-Migration InitialCreate` e `Update-Database`.) Faça commit da nova pasta `Migrations/`.

## Endpoints

| Método | Rota | Sucesso | Erros |
|--------|------|---------|-------|
| GET | `/api/dispositivos` | 200 | |
| GET | `/api/dispositivos/{id}` | 200 | 404 |
| GET | `/api/dispositivos/{id}/eventos` | 200 | 404 |
| POST | `/api/dispositivos` | 201 + `Location` | 400 |
| PUT | `/api/dispositivos/{id}` | 204 | 400, 404 |
| DELETE | `/api/dispositivos/{id}` | 204 (apaga os eventos em cascata) | 404 |
| GET | `/api/eventos` (filtro opcional `?dispositivoId=1`) | 200 | |
| GET | `/api/eventos/{id}` | 200 | 404 |
| POST | `/api/eventos` (endpoint que o ESP32 chama) | 201 + `Location` | 400 |
| PUT | `/api/eventos/{id}` | 204 | 400, 404 |
| DELETE | `/api/eventos/{id}` | 204 | 404 |

Regras extras:

- Um evento `HEARTBEAT` atualiza `ULTIMO_HEARTBEAT` do dispositivo e muda o status para `ONLINE`.
- Tipo de dispositivo/evento inválido, campo obrigatório vazio, valor fora da faixa ou `dispositivoId` inexistente devolvem **400** com mensagem em JSON.
- Id inexistente devolve **404** com mensagem em JSON.
- Erros inesperados devolvem **500** em JSON, sem detalhes internos.

Exemplo de erro 400:

```json
{ "mensagem": "tipo inválido. Use: LAMPADA, BASE." }
```

```json
{ "mensagem": "Dados inválidos.", "erros": { "Comodo": ["comodo é obrigatório."] } }
```

## Como testar

O arquivo `requests.http` tem todas as chamadas prontas (VS Code com a extensão REST Client, ou Rider/Visual Studio). Com `curl`:

```bash
# 1) cadastrar um dispositivo (201)
curl -i -X POST http://localhost:5000/api/dispositivos \
  -H "Content-Type: application/json" \
  -d '{"idosoId":1,"tipo":"LAMPADA","comodo":"Sala"}'

# 2) enviar um evento como o ESP32 faz (201)
curl -i -X POST http://localhost:5000/api/eventos \
  -H "Content-Type: application/json" \
  -d '{"dispositivoId":1,"tipo":"PRESENCA_PARADA","valor":1,"dataHora":"2026-11-12T07:42:10-03:00"}'

# 3) heartbeat: o dispositivo fica ONLINE
curl -i -X POST http://localhost:5000/api/eventos \
  -H "Content-Type: application/json" \
  -d '{"dispositivoId":1,"tipo":"HEARTBEAT","valor":1}'
curl http://localhost:5000/api/dispositivos/1

# 4) erros
curl -i -X POST http://localhost:5000/api/eventos -H "Content-Type: application/json" \
  -d '{"dispositivoId":1,"tipo":"XYZ","valor":1}'          # 400
curl -i http://localhost:5000/api/dispositivos/9999        # 404
```

Para conferir a persistência, consulte direto no Oracle:

```sql
SELECT * FROM DISPOSITIVO;
SELECT * FROM EVENTO ORDER BY DATA_HORA DESC;
```

## Estrutura

```
.
├── Controllers/    DispositivosController, EventosController
├── Data/           OracleConnection, DesignTimeDbContextFactory
├── Dtos/           requisições e respostas (com validações)
├── Models/         Dispositivo, Evento, Contexto (DbContext)
├── Migrations/     InitialCreate + snapshot (versionadas no Git)
├── Program.cs
├── TOBem.TelemetriaApi.sln / .csproj
└── requests.http
```

