-- FootballTracker – Supabase Schema
-- Im Supabase Dashboard unter "SQL Editor" ausführen. Das Skript ist idempotent
-- und kann auch auf einer bestehenden Datenbank erneut ausgeführt werden.

-- ---------------------------------------------------------------------------
-- Benutzerprofile
-- ---------------------------------------------------------------------------
create table if not exists public.users (
    id        uuid primary key references auth.users (id) on delete cascade,
    username  text,
    vorname   text,
    nachname  text
);

alter table public.users enable row level security;

drop policy if exists "users_select_own" on public.users;
create policy "users_select_own" on public.users
    for select using (auth.uid() = id);

drop policy if exists "users_insert_own" on public.users;
create policy "users_insert_own" on public.users
    for insert with check (auth.uid() = id);

drop policy if exists "users_update_own" on public.users;
create policy "users_update_own" on public.users
    for update using (auth.uid() = id);

-- Legt bei der Registrierung automatisch ein Profil an. Die Felder werden aus den
-- beim SignUp übergebenen Metadaten (username, vorname, nachname) übernommen.
create or replace function public.handle_new_user()
returns trigger
language plpgsql
security definer set search_path = public
as $$
begin
    insert into public.users (id, username, vorname, nachname)
    values (
        new.id,
        new.raw_user_meta_data ->> 'username',
        new.raw_user_meta_data ->> 'vorname',
        new.raw_user_meta_data ->> 'nachname'
    )
    on conflict (id) do nothing;
    return new;
end;
$$;

drop trigger if exists on_auth_user_created on auth.users;
create trigger on_auth_user_created
    after insert on auth.users
    for each row execute procedure public.handle_new_user();

-- ---------------------------------------------------------------------------
-- Favorisierte Spieler (Player Tracker)
-- ---------------------------------------------------------------------------
create table if not exists public.favorite_players (
    id          bigint generated always as identity primary key,
    user_id     uuid not null references auth.users (id) on delete cascade,
    player_id   integer not null,
    created_at  timestamptz not null default now()
);

alter table public.favorite_players add column if not exists player_name text;

-- Doppelte Einträge aus älteren App-Versionen entfernen, bevor der Unique-Index greift.
delete from public.favorite_players a
    using public.favorite_players b
    where a.ctid < b.ctid
      and a.user_id = b.user_id
      and a.player_id = b.player_id;

create unique index if not exists favorite_players_user_player_idx
    on public.favorite_players (user_id, player_id);

alter table public.favorite_players enable row level security;

drop policy if exists "favorite_players_own" on public.favorite_players;
create policy "favorite_players_own" on public.favorite_players
    for all using (auth.uid() = user_id) with check (auth.uid() = user_id);

-- ---------------------------------------------------------------------------
-- Favorisierte Teams
-- ---------------------------------------------------------------------------
create table if not exists public.favorite_teams (
    id             bigint generated always as identity primary key,
    user_id        uuid not null references auth.users (id) on delete cascade,
    team_id        integer not null,
    team_name      text,
    team_icon_url  text,
    created_at     timestamptz not null default now()
);

create unique index if not exists favorite_teams_user_team_idx
    on public.favorite_teams (user_id, team_id);

alter table public.favorite_teams enable row level security;

drop policy if exists "favorite_teams_own" on public.favorite_teams;
create policy "favorite_teams_own" on public.favorite_teams
    for all using (auth.uid() = user_id) with check (auth.uid() = user_id);
