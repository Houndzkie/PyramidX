-- ====================================================================
-- SUPABASE DATABASE SETUP FOR PYRAMIDS OF EGYPT (PYRAMIDX)
-- ====================================================================
-- Instructions:
-- 1. Open your Supabase Dashboard: https://supabase.com/dashboard
-- 2. Select your Project -> Click on "SQL Editor" in the left sidebar
-- 3. Click "New Query", paste this entire script, and click "Run"
-- ====================================================================

-- 1. Grant Schema Permissions to anon and authenticated roles
GRANT USAGE ON SCHEMA public TO anon, authenticated;

-- 2. Create Pyramids Table
CREATE TABLE IF NOT EXISTS public.pyramids (
    id SERIAL PRIMARY KEY,
    name TEXT NOT NULL,
    location TEXT NOT NULL,
    height NUMERIC NOT NULL,
    built_era TEXT NOT NULL,
    description TEXT NOT NULL,
    entry_fee_usd NUMERIC NOT NULL,
    image_url TEXT NOT NULL,
    rating NUMERIC NOT NULL DEFAULT 5.0,
    created_at TIMESTAMPTZ DEFAULT NOW()
);

-- 3. Create Users Table
CREATE TABLE IF NOT EXISTS public.users (
    id SERIAL PRIMARY KEY,
    username TEXT NOT NULL UNIQUE,
    email TEXT NOT NULL UNIQUE,
    password TEXT NOT NULL,
    preferences TEXT[] DEFAULT '{}',
    created_at TIMESTAMPTZ DEFAULT NOW()
);

-- 4. Create Tour Bookings Table
CREATE TABLE IF NOT EXISTS public.tour_bookings (
    id SERIAL PRIMARY KEY,
    pyramid_id INT REFERENCES public.pyramids(id) ON DELETE SET NULL,
    pyramid_name TEXT NOT NULL,
    tour_date DATE NOT NULL,
    number_of_tickets INT NOT NULL DEFAULT 1,
    total_price NUMERIC NOT NULL,
    status TEXT NOT NULL DEFAULT 'Confirmed',
    created_at TIMESTAMPTZ DEFAULT NOW()
);

-- 5. Grant Full Table & Sequence Permissions to anon and authenticated
GRANT ALL ON ALL TABLES IN SCHEMA public TO anon, authenticated;
GRANT ALL ON ALL SEQUENCES IN SCHEMA public TO anon, authenticated;
GRANT ALL ON ALL ROUTINES IN SCHEMA public TO anon, authenticated;

ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON TABLES TO anon, authenticated;
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON SEQUENCES TO anon, authenticated;

-- 6. Disable Row Level Security (RLS) for demo/educational purposes
-- This allows the anon key to read, register users, and book tours without policy blocks
ALTER TABLE public.pyramids DISABLE ROW LEVEL SECURITY;
ALTER TABLE public.users DISABLE ROW LEVEL SECURITY;
ALTER TABLE public.tour_bookings DISABLE ROW LEVEL SECURITY;

-- 7. Seed Initial Promoted Pyramids
INSERT INTO public.pyramids (id, name, location, height, built_era, description, entry_fee_usd, image_url, rating)
VALUES 
(1, 'Great Pyramid of Giza', 'Giza Plateau, Greater Cairo', 146.6, 'Old Kingdom, 4th Dynasty (~2560 BC)', 
 'The monumental tomb of Pharaoh Khufu. The oldest and sole surviving wonder of the original Seven Wonders of the Ancient World, constructed with over 2.3 million megalithic limestone blocks.', 
 25.00, 'https://upload.wikimedia.org/wikipedia/commons/thumb/e/e7/Great_Pyramid_of_Giza_-_Pyramid_of_Khufu.jpg/1280px-Great_Pyramid_of_Giza_-_Pyramid_of_Khufu.jpg', 4.95),

(2, 'Pyramid of Khafre', 'Giza Plateau, Greater Cairo', 136.4, 'Old Kingdom, 4th Dynasty (~2570 BC)', 
 'Built for Pharaoh Khafre, son of Khufu. Perched on higher bedrock and still crowned by its lustrous original polished Tura limestone casing stones, guarded by the iconic Great Sphinx.', 
 20.00, 'https://upload.wikimedia.org/wikipedia/commons/6/69/Pyramid_of_Khafre_and_Sphinx%2C_Giza%2C_Greater_Cairo%2C_Egypt.jpg', 4.88),

(3, 'Step Pyramid of Djoser', 'Saqqara Necropolis, Giza', 62.5, 'Old Kingdom, 3rd Dynasty (~2670 BC)', 
 'Designed by the legendary architect Imhotep for Pharaoh Djoser. Widely celebrated as humanity''s very first colossal stone monument, pioneering a revolutionary six-tiered mastaba design.', 
 18.00, 'https://upload.wikimedia.org/wikipedia/commons/thumb/6/6a/Sakkara_01.jpg/1280px-Sakkara_01.jpg', 4.82)
ON CONFLICT (id) DO UPDATE SET
  name = EXCLUDED.name,
  location = EXCLUDED.location,
  height = EXCLUDED.height,
  built_era = EXCLUDED.built_era,
  description = EXCLUDED.description,
  entry_fee_usd = EXCLUDED.entry_fee_usd,
  image_url = EXCLUDED.image_url,
  rating = EXCLUDED.rating;

-- 8. Seed Demo User
INSERT INTO public.users (id, username, email, password, preferences)
VALUES 
(1, 'RamsesExplorer', 'ramses@pyramids.eg', 'Password123!', ARRAY['Archaeology', 'Sunset Tours', 'VIP Tomb Access', 'Photography'])
ON CONFLICT (id) DO NOTHING;

-- 9. Seed Initial Tour Bookings
INSERT INTO public.tour_bookings (id, pyramid_id, pyramid_name, tour_date, number_of_tickets, total_price, status)
VALUES 
(101, 1, 'Great Pyramid of Giza', CURRENT_DATE + INTERVAL '2 days', 2, 50.00, 'Confirmed'),
(102, 3, 'Step Pyramid of Djoser', CURRENT_DATE + INTERVAL '5 days', 3, 54.00, 'Confirmed')
ON CONFLICT (id) DO NOTHING;

-- 10. Update serial sequence counters to prevent ID collisions on new inserts
SELECT setval(pg_get_serial_sequence('public.pyramids', 'id'), COALESCE(MAX(id), 1)) FROM public.pyramids;
SELECT setval(pg_get_serial_sequence('public.users', 'id'), COALESCE(MAX(id), 1)) FROM public.users;
SELECT setval(pg_get_serial_sequence('public.tour_bookings', 'id'), COALESCE(MAX(id), 1)) FROM public.tour_bookings;
