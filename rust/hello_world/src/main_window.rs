use gtk::prelude::*;
use gtk::{Application, ApplicationWindow, Builder, Button, Label, MenuItem};
use std::cell::RefCell;
use std::rc::Rc;

const TEXT: &str = "Hello World";
const SCROLL_SPEED: u32 = 50; // milliseconds
const LABEL_WIDTH: usize = 40; // characters

pub struct MainWindow {
    window: ApplicationWindow,
}

impl MainWindow {
    pub fn new(app: &Application) -> Self {
        // Load UI file from filesystem at runtime
        let ui_file = std::path::Path::new("main_window.ui");
        let builder = if ui_file.exists() {
            Builder::from_file(ui_file)
        } else {
            // Fallback: try to load from src directory during development
            Builder::from_file("src/main_window.ui")
        };

        let window: ApplicationWindow = builder
            .object("MainWindow")
            .expect("Failed to find MainWindow");

        window.set_application(Some(app));

        let scrolling_label: Label = builder
            .object("scrollingLabel")
            .expect("Failed to find scrollingLabel");

        let exit_button: Button = builder
            .object("exitButton")
            .expect("Failed to find exitButton");

        let quit_menu_item: MenuItem = builder
            .object("quitMenuItem")
            .expect("Failed to find quitMenuItem");

        // Setup exit button click handler
        let window_clone = window.clone();
        exit_button.connect_clicked(move |_| {
            window_clone.close();
        });

        // Setup quit menu item activation handler
        let window_clone = window.clone();
        quit_menu_item.connect_activate(move |_| {
            window_clone.close();
        });

        // Setup scrolling animation
        let scroll_position = Rc::new(RefCell::new(0usize));
        let label_clone = scrolling_label.clone();

        glib::timeout_add_local(
            std::time::Duration::from_millis(SCROLL_SPEED as u64),
            move || {
                let mut pos = scroll_position.borrow_mut();
                *pos += 1;

                let text_length = TEXT.len();
                let total_length = LABEL_WIDTH + text_length;
                let current_pos = *pos % total_length;

                let display = create_scrolling_text(current_pos, text_length);
                label_clone.set_text(&display);

                glib::ControlFlow::Continue
            },
        );

        MainWindow { window }
    }

    pub fn show(&self) {
        self.window.show_all();
    }
}

fn create_scrolling_text(pos: usize, text_length: usize) -> String {
    let mut display = " ".repeat(LABEL_WIDTH);

    if pos < LABEL_WIDTH {
        let start_pos = LABEL_WIDTH - pos;
        let visible_chars = std::cmp::min(text_length, pos);

        if visible_chars > 0 {
            let text_slice = &TEXT[..visible_chars];
            display.replace_range(start_pos..start_pos + visible_chars, text_slice);
        }
    } else {
        let text_pos = pos - LABEL_WIDTH;
        let remaining_chars = text_length - text_pos;

        if remaining_chars > 0 {
            let text_slice = &TEXT[text_pos..];
            display.replace_range(..remaining_chars, text_slice);
        }
    }

    display
}
