# Dogtor Burguer! - Game Design Document

## Descripcion General

**Dogtor Burguer!** es un juego arcade para moviles inspirado en el clasico *Yoshi* de NES. El jugador controla a un chef-perro que debe gestionar platos giratorios para combinar ingredientes de hamburguesas que caen desde arriba. El objetivo es completar hamburguesas locas, acumular puntos y evitar que los ingredientes lleguen al tope.

---

## Mecanicas Principales

### Estructura

- Vista en vertical
- 4 columnas fijas (platos)
- 3 posiciones posibles para el chef (ocupa 2 platos a la vez)

### Ingredientes

- Total: 7 ingredientes
  - Carne
  - Queso americano
  - Tomate
  - Cebolla
  - Pepinillo
  - Lechuga
  - Huevo frito (ingrediente especial, aparece en fases avanzadas)
- Pan inferior y pan superior (para cerrar hamburguesas)
  - Pan superior con semillas
  - Pan inferior muestra la miga

### Logica de Caida

- Los ingredientes caen de forma escalonada (casillas), no fluida.
- Se acumulan formando torres en cada plato.
- Si caen dos ingredientes del mismo tipo uno sobre otro → **se eliminan**.
- Pan inferior + ingredientes + pan superior → **se forma una hamburguesa** y se elimina toda la pila intermedia.

---

## Control del Chef (Dogtor)

- El jugador puede:
  - Mover al chef tocando una de las 3 posiciones validas (cambia de columna).
  - Tocar sobre el chef para girar los dos platos donde esta posicionado.

---

## Progresion y Dificultad

- El juego comienza con solo 4 ingredientes.
- Se van anadiendo mas ingredientes con el tiempo (hasta los 7).
- La velocidad de caida aumenta progresivamente.

---

## Puntuacion

- Juntar 2 ingredientes iguales: +10 puntos
- Hamburguesa cerrada:
  - 10 pts por ingrediente + bonus por tamano
  - Nombres locos generados en funcion de la cantidad de ingredientes

---

## Nombres de Hamburguesas

- Pool predefinido de nombres como:
  - "La Antitorre Explosiva"
  - "Mega Trapo Deluxe"
  - "Doble Grito Vegano"
- Cuanto mas grande la hamburguesa, mas absurdo el nombre.

---

## Monetizacion

- Anuncios entre partidas
- Recompensa por continuar partida (ver anuncio = limpiar la mesa)
- Sistema de "gemas" como moneda blanda
  - Ver anuncios otorga gemas
  - Gastar gemas permite continuar tras perder

---

## Estilo Visual

- Sprites con estilo cartoon cutre pero brillante (tipo Pizza Tower)
- Trazos gruesos, formas irregulares, estetica absurda pero reconocible
- Ingredientes con colores vivos y contornos marcados
