# ADR 0004: MassTransit 8.3.4 em vez da versão 9

## Status

Aceito

## Contexto

A partir da linha MassTransit **9**, o uso em produção exige licença comercial configurada via `MT_LICENSE` ou `SetLicense`. Sem licença, a aplicação falha na inicialização do bus (`MassTransit.ConfigurationException`).

Este repositório é um **projeto de portfólio e base reutilizável** que deve:

- Subir com `docker compose up` sem segredos ou licenças externas
- Permanecer reproduzível em máquinas de quem clona o repositório
- Manter código aberto e documentação clara

A versão **8.3.4** permanece sob licença Apache 2.0 e não exige chave de licença.

## Decisão

Fixar `MassTransit.RabbitMQ` na versão **8.3.4** em `BuildingBlocks.Messaging`.

Não adotar MassTransit 9 neste repositório até haver requisito explícito de recursos exclusivos da v9 e licenciamento definido.

## Alternativas consideradas

| Alternativa | Motivo de rejeição |
|-------------|-------------------|
| MassTransit 9 + licença community | Adiciona passo de configuração e variável de ambiente não adequada para demo zero-config |
| Outro broker/cliente raw | Perde convenções, retry e endpoints já padronizados no projeto |
| MassTransit 9 sem licença | Não inicia; inviável para Docker Compose |

## Consequências

- Compatível com Docker Compose e CI sem configuração extra
- API e contratos MassTransit 8 permanecem estáveis até dez/2026 (manutenção documentada pelo projeto)
- Migração futura para v9 exigirá ADR, licença e validação de breaking changes

## Referências

- [MassTransit — License configuration](https://masstransit.io/documentation/configuration/license)
- ADR 0002 — RabbitMQ com MassTransit
