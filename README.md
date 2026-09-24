# World Construções

Aplicação ASP.NET Core MVC para anúncios de imóveis, atendimento de corretores e acompanhamento privado de obras. A interface usa como referência visual o azul-marinho e o verde-água do site estático World Construções.

## Perfis

- **Visitante:** consulta somente anúncios publicados e envia uma mensagem sobre um imóvel.
- **Cliente:** acompanha obras vinculadas à própria conta e conversa sobre seus contatos.
- **Corretor:** atende apenas contatos atribuídos a ele.
- **Administrador:** publica imóveis, atribui corretores e clientes, cadastra corretores e gerencia fotos e atualizações de obra.

As permissões são aplicadas nos controllers no servidor. Fotos privadas ficam fora de wwwroot e são entregues por uma rota autenticada. O upload aceita JPG, PNG e WebP de até 8 MB. O primeiro acesso cria as tabelas e os perfis de usuário com \`EnsureCreated\`; para evoluir o esquema de um banco já em uso, adicione migrations antes de produção.

## Executar no Visual Studio

1. Abra \`World Consntrucoes.slnx\`.
2. Confira a conexão SQL Server em \`ConnectionStrings:DefaultConnection\` ou defina \`ConnectionStrings__DefaultConnection\`.
3. Configure o primeiro administrador com User Secrets ou variáveis de ambiente:

~~~powershell
dotnet user-secrets set "BootstrapAdmin:Email" "admin@worldconstrucoes.com"
dotnet user-secrets set "BootstrapAdmin:Password" "SUA-SENHA-FORTE"
~~~

O banco local padrão usa SQL Server LocalDB. A senha não deve ser colocada no repositório. Se o e-mail do administrador inicial já existir como outra conta, a aplicação interrompe a inicialização em vez de promover essa conta automaticamente.

## Primeiros passos

1. Entre com a conta administradora configurada.
2. Cadastre os corretores.
3. Crie imóveis, atribua o corretor responsável e publique os anúncios.
4. Cadastre o cliente pelo formulário público e, no painel administrativo do imóvel, vincule a conta.
5. O cliente pode enviar mensagens pela página pública e continuar a conversa pela área do cliente.
6. Visitantes podem usar o formulário do imóvel ou os links de e-mail/telefone do corretor. Solicitações ficam na caixa de atendimentos; envio automático de e-mail ainda não está configurado.

Os uploads ficam em \`App_Data/uploads\`, fora de wwwroot, e devem usar armazenamento persistente no ambiente de publicação. Faça backup desse diretório junto com o banco de dados.