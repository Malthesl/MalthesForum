# Web API

Her er en lille oversigt over alle endpoints.

## Opslag

### Hent et opslag

URL: `GET /posts/{id}`

### Opret et opslag

URL: `POST /posts`

### Opdater et opslag

URL: `POST /posts/{id}`

### Slet et opslag

URL: `DELETE /posts/{id}`

### Hent alle kommentarer på et opslag

URL: `GET /posts/{id}/comments`

### Hent posts med filtre (GetMany)

URL: `GET /posts?subforumId=...&userId=...&search=...&before=...&after=...&commentDepth=...&limit=...&offset=...`

### Hent posts fra et subforum

URL: `GET /posts?subforumId={subforumId}`

## Subforum

### Hent alle subforums

URL: `GET /subforums`

### Opret et subforum

URL: `POST /subforums`

### Hent oplysninger om et subforum

URL: `GET /subforums/{id}`

### Hent oplysninger om et subforum (via dens url navn)

URL: `GET /subforums/{urlName}`

### Opdater oplysninger om et subforum

URL: `POST /subforums/{id}`

### Slet et subforum

URL: `DELETE /subforums/{id}`

### Hent subforums med filtre (GetMany)

URL: `GET /subforums?search=...&moderatedByUserId=...`

## Brugere

### Hent offentlige oplysninger om en bruger

URL: `GET /users/{id}`

### Opret en bruger

URL: `POST /users/register`

### Opdater en bruger

URL: `POST /users/{id}`

### Slet en bruger

URL: `DELETE /users/{id}`

### Hent brugere med filtre (GetMany)

URL: `GET /users?search=...&limit=...&offset=...`

## Reactions

### Like et opslag

URL: `POST /posts/{id}/like`

### Fjern like på et opslag

URL: `DELETE /posts/{id}/like`

### Dislike et opslag

URL: `POST /posts/{id}/dislike`

### Fjern dislike på et opslag

URL: `DELETE /posts/{id}/dislike`