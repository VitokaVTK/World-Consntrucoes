# World Construções

Aplicação ASP.NET Core MVC para anúncios de imóveis, atendimento de corretores e acompanhamento privado de obras. A interface usa como referência visual o azul-marinho e o verde-água do site estático World Construções.

## Perfis

- **Visitante:** consulta somente anúncios publicados e envia uma mensagem sobre um imóvel.
- **Cliente:** acompanha obras vinculadas à própria conta e conversa sobre seus contatos.
- **Corretor:** atende apenas contatos atribuídos a ele.
- **Administrador:** publica imóveis, atribui corretores e clientes, cadastra corretores e gerencia fotos e atualizações de obra.

As permissões são aplicadas nos controllers no servidor. Fotos privadas ficam fora de wwwroot e são entregues por uma rota autenticada. O upload aceita JPG, PNG e WebP de até 8 MB.

## Executar no Visual Studio

1. Abra \`World Consntrucoes.slnx\`.
2. Confira a conexão SQL Server em \`ConnectionStrings:DefaultConnection\` ou defina \`ConnectionStrings__DefaultConnection\`.
3. Configure o primeiro administrador com User Secrets ou variáveis de ambiente:

~~~powershell
dotnet user-secrets set "BootstrapAdmin:Email" "admin@worldconstrucoes.com"
dotnet user-secrets set "BootstrapAdmin:Password" "SUA-SENHA-FORTE"
~~~

O banco local padrão usa SQL Server LocalDB. Na primeira execução a aplicação cria as tabelas e os perfis Administrador, Corretor e Cliente. Para outro SQL Server, substitua a connection string com uma variável de ambiente; não coloque senha no repositório.

## Primeiros passos

1. Entre com a conta administradora configurada.
2. Cadastre os corretores.
3. Crie imóveis e atribua um corretor.
4. Publique os anúncios desejados.
5. Cadastre o cliente pelo formulário público e, no painel administrativo do imóvel, vincule o e-mail do cliente para mostrar atualizações privadas da obra.
6. Envie uma mensagem de teste pela página pública do imóvel e acompanhe a conversa na caixa de entrada do corretor.

Os uploads ficam em \`App_Data/uploads\`; faça backup desse diretório junto com o banco de dados.