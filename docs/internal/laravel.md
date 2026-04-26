# BeaverTech Tracker - Laravel (publicacao)

Publicacao do pacote `beavertech/tracker-laravel`.

## Preparar release

1. Atualize `sdk/laravel/composer.json` (versao e descricao).
2. Garanta que o `README.md` esta atualizado.
3. Crie um tag git com a versao (ex: `v0.1.0`).

## Publicar no Packagist

1. Hospede o codigo em um repositorio git publico (GitHub/GitLab).
2. No Packagist, clique em "Submit" e informe a URL do repo.
3. Configure o webhook para atualizacoes automaticas.

## Publicar em repositorio privado (opcional)

- Use Satis/Private Packagist e aponte para o repositorio.
- No projeto consumidor, configure o repositorio:

```bash
composer config repositories.beavertech-tracker composer https://seu-repo
composer require beavertech/tracker-laravel
```
