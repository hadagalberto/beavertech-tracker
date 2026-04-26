# BeaverTech Tracker - Node.js (publicacao)

Publicacao do pacote `@beaver-tech/tracker`.

## Preparar release

1. Atualize a versao em `sdk/node/package.json`.
2. Revise `sdk/node/README.md`.
3. Faça tag git (ex: `v0.1.0`).

## Publicar no npm

```bash
cd sdk/node
npm login
npm publish
```

## Publicar em registry privado

```bash
npm login --registry https://seu-registry
npm publish --registry https://seu-registry
```
