# ER-діаграма бази даних

Діаграма побудована з реальної схеми PostgreSQL, яку створюють міграції `UsersAndPlatformTables`, `LotsAndCharityTables` і `BiddingTables` (SITE-14, SITE-15, SITE-16). Типи, ключі й зв'язки взято з системного каталогу бази, а не з коду, тому вони збігаються з тим, що отримає сервер після `migrate`.

Позначки: `PK` первинний ключ, `FK` зовнішній ключ, `UK` унікальне значення. Зв'язок `||--o{` означає «один до багатьох», `|o--o{` те саме з необов'язковим посиланням, `||--o|` один до нуля або одного.

## Діаграма

```mermaid
erDiagram
    users ||--o{ admin_actions : "admin_id"
    lots ||--o{ complaints : "lot_id"
    users ||--o{ complaints : "author_id"
    users |o--o{ complaints : "reviewed_by"
    users ||--o{ sessions : "user_id"
    users ||--o{ user_tokens : "user_id"
    users |o--o{ donation_links : "reviewed_by"
    users ||--o{ donation_links : "submitted_by"
    categories ||--o{ lots : "category_id"
    donation_links |o--o{ lots : "donation_link_id"
    lots |o--o{ lots : "source_lot_id"
    users |o--o{ lots : "leader_id"
    users ||--o{ lots : "seller_id"
    users |o--o{ lots : "winner_id"
    lots ||--o| donation_reports : "lot_id"
    users ||--o{ donation_reports : "winner_id"
    lots ||--o{ favorites : "lot_id"
    users ||--o{ favorites : "user_id"
    lots ||--o{ lot_photos : "lot_id"
    lots ||--o{ lot_views : "lot_id"
    lots ||--o{ bids : "lot_id"
    users ||--o{ bids : "bidder_id"
    users ||--o{ notifications : "recipient_id"
    lots |o--o{ suspicious_activities : "lot_id"
    users ||--o{ suspicious_activities : "user_id"

    admin_actions {
        bigint id PK
        bigint admin_id FK
        text action
        text target_type
        bigint target_id
        text reason
        timestamptz created_at
    }
    analytics_events {
        bigint id PK
        text type
        bigint user_id
        bigint lot_id
        text visitor_key
        jsonb payload
        timestamptz created_at
    }
    users {
        bigint id PK
        citext email UK
        text password_hash
        timestamptz email_confirmed_at
        citext nickname UK
        text phone
        varchar50 city
        text avatar_key
        text role
        text status
        bool email_notifications
        timestamptz created_at
    }
    sessions {
        bigint id PK
        bigint user_id FK
        bytea token_hash UK
        timestamptz created_at
        timestamptz last_seen_at
        timestamptz expires_at
        varchar512 user_agent
    }
    user_tokens {
        bigint id PK
        bigint user_id FK
        text purpose
        bytea token_hash UK
        timestamptz created_at
        timestamptz expires_at
        timestamptz used_at
    }
    complaints {
        bigint id PK
        bigint author_id FK
        bigint lot_id FK
        text reason
        varchar1000 comment
        text status
        timestamptz created_at
        bigint reviewed_by FK
        timestamptz reviewed_at
        text review_comment
    }
    categories {
        smallint id PK
        text name UK
        smallint sort_order
    }
    lots {
        bigint id PK
        bigint seller_id FK
        smallint category_id FK
        varchar100 title
        varchar5000 description
        text condition
        bigint start_price
        bigint bid_step
        int duration_minutes
        text status
        bool is_hidden
        text hidden_reason
        timestamptz hidden_at
        text donation_url
        bigint donation_link_id FK
        text review_rejection_reason
        bigint source_lot_id FK
        timestamptz created_at
        timestamptz submitted_at
        timestamptz started_at
        timestamptz ends_at
        timestamptz closed_at
        bigint current_price
        bigint leader_id FK
        int bid_count
        int bidder_count
        bigint winner_id FK
        bigint final_price
        int view_count
    }
    lot_photos {
        bigint id PK
        bigint lot_id FK
        smallint position
        text large_key
        text thumb_key
        int width
        int height
        timestamptz created_at
    }
    favorites {
        bigint user_id PK, FK
        bigint lot_id PK, FK
        timestamptz created_at
        timestamptz reminder_sent_at
    }
    lot_views {
        bigint lot_id PK, FK
        text visitor_key PK
        date view_date PK
    }
    donation_links {
        bigint id PK
        text normalized_url UK
        text original_url
        text status
        bigint submitted_by FK
        timestamptz created_at
        bigint reviewed_by FK
        timestamptz reviewed_at
        text review_reason
    }
    donation_reports {
        bigint lot_id PK, FK
        bigint winner_id FK
        bigint amount
        text receipt_key
        timestamptz reported_at
    }
    bids {
        bigint id PK
        bigint lot_id FK
        bigint bidder_id FK
        bigint amount
        timestamptz placed_at
    }
    notifications {
        bigint id PK
        bigint recipient_id FK
        text type
        bigint lot_id
        text text
        text link
        bool is_read
        timestamptz created_at
        text email_status
        int email_attempts
        timestamptz email_next_attempt_at
        timestamptz email_sent_at
    }
    suspicious_activities {
        bigint id PK
        bigint user_id FK
        bigint lot_id FK
        text type
        jsonb details
        timestamptz created_at
        bool is_reviewed
    }
```

## Таблиці за модулями

| Модуль | Таблиці |
|---|---|
| Platform | `admin_actions`, `analytics_events` |
| Users | `users`, `sessions`, `user_tokens`, `complaints` |
| Lots | `categories`, `lots`, `lot_photos`, `favorites`, `lot_views`, `donation_links`, `donation_reports` |
| Bidding | `bids`, `notifications`, `suspicious_activities` |

Усього 16 таблиць і 25 зовнішніх ключів. Правило видалення `restrict` мають 18 ключів, `cascade` такі:

- `sessions.user_id` → `users`
- `user_tokens.user_id` → `users`
- `favorites.lot_id` → `lots`
- `favorites.user_id` → `users`
- `lot_photos.lot_id` → `lots`
- `lot_views.lot_id` → `lots`
- `notifications.recipient_id` → `users`

## Перевірка закладок під v2.0 (project.md §9.13)

Висновок: схема не блокує жодну функцію v2.0. Для кожної потрібні нові таблиці або необов'язкові колонки, переписувати наявні таблиці не доведеться. Нижче наведено, що вже закладено і що доведеться змінити.

| Функція | Ключ | Закладено | Що додати або вирішити |
|---|---|---|---|
| Автоставки до максимуму | SITE-39 | `bids` зберігає автора (`bidder_id`) і суму (`amount`), тож автоставка є звичайною ставкою від імені користувача. `UNIQUE (lot_id, amount)` їй не заважає | Нова таблиця `auto_bids (lot_id, user_id, max_amount)`; таблицю `bids` змінювати не потрібно |
| «Купити зараз» | SITE-40 | Лот має `winner_id` і `final_price`, які можна заповнити без ставки | Необов'язкова колонка `lots.buy_now_price`. Якщо викуп завершує лот зі статусом `sold`, інших змін немає. Якщо потрібен новий статус, доведеться змінити `ck_lots_status` і `ck_lots_final_price_when_closed` |
| Рейтинг і відгуки | SITE-41 | `lots.seller_id` і `lots.winner_id` для лота `sold` однозначно задають сторони угоди, а `donation_reports` додає підтвердження переможця | Нова таблиця `reviews` з посиланням на лот і автора |
| Аналітика цін | SITE-42 | `lots.final_price`, `category_id`, `condition` і `closed_at` лишаються назавжди: у моделі немає видалення лотів, а ключі зі ставок, скарг і звітів мають `restrict`. Індекс `(status, closed_at desc)` вже є | Індекс `(category_id, closed_at)`, коли з'являться запити по категорії. Бажано заборонити `DELETE` на `lots` правами ролі БД, бо фото, обрані й перегляди видаляються каскадом разом з лотом |
| Оплата і просування | SITE-43 | Комісію 5% можна рахувати від `final_price`, колонка вже зберігається | Необов'язкова колонка `lots.promoted_until`, таблиця платежів. **Відкрите питання:** за `design/14-charity.md` 100% фінальної ціни йде на збір (відхилення X1), а v2.0 передбачає комісію; модель комісії треба узгодити з командою |
| PWA | SITE-44 | Сповіщення вже окрема таблиця `notifications` | Стан доставки в ній прив'язаний до листів (`email_status`, `email_attempts`, `email_next_attempt_at`, `email_sent_at` і їхні `CHECK`). Для push краще нова таблиця `notification_deliveries (notification_id, channel, status, attempts)` і таблиця підписок пристроїв, а не нові колонки |
| Рекомендації схожих лотів | — | `lots.category_id` і `condition`, індекс `(category_id, status, is_hidden)`, а також `favorites` і `lot_views` як сигнали | Нічого не потрібно для першої версії рекомендацій |

Дата побудови: 2026-11-16.
