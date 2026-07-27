(function () {
    "use strict";

    document.addEventListener("DOMContentLoaded", function () {
        const form = document.getElementById("weeklyAssignmentForm");
        if (!form) {
            return;
        }

        const statusSelect = document.getElementById("assignmentStatus");
        const statusHelp = document.getElementById("assignmentStatusHelp");
        const submitButton = document.getElementById("assignmentSubmitButton");
        const selectAll = document.getElementById("selectAllLessons");
        const selectedCount = document.getElementById("selectedLessonCount");
        const selectedList = document.getElementById("selectedLessonsList");
        const emptySelection = document.getElementById("selectedLessonsEmpty");
        const lessonCheckboxes = Array.from(form.querySelectorAll(".lesson-check"));
        const confirmationElement = document.getElementById("publishAssignmentConfirmation");
        const confirmationModal = confirmationElement
            ? bootstrap.Modal.getOrCreateInstance(confirmationElement)
            : null;
        let publishConfirmed = false;

        function selectedLessons() {
            return lessonCheckboxes
                .filter(checkbox => checkbox.checked)
                .map(checkbox => {
                    const picker = form.querySelector(`[data-content-picker="${checkbox.value}"]`);
                    const countSelected = kind => {
                        if (!picker) {
                            return 0;
                        }

                        return Array.from(picker.querySelectorAll(`.content-item-check[data-kind="${kind}"]:checked`))
                            .filter(item => item.closest(".assignment-content-group")
                                ?.querySelector(".activity-type-check")?.checked)
                            .length;
                    };

                    return {
                        id: checkbox.value,
                        title: checkbox.dataset.title || "",
                        duration: Number(checkbox.dataset.duration || 0),
                        xp: Number(checkbox.dataset.xp || 0),
                        vocabulary: countSelected("vocabulary"),
                        quiz: countSelected("quiz"),
                        games: countSelected("game"),
                        previewUrl: checkbox.dataset.previewUrl || ""
                    };
                });
        }

        function totals(lessons) {
            return lessons.reduce((result, lesson) => {
                result.duration += lesson.duration;
                result.xp += lesson.xp;
                result.vocabulary += lesson.vocabulary;
                result.quiz += lesson.quiz;
                result.games += lesson.games;
                return result;
            }, { duration: 0, xp: 0, vocabulary: 0, quiz: 0, games: 0 });
        }

        function setText(id, value) {
            const element = document.getElementById(id);
            if (element) {
                element.textContent = String(value);
            }
        }

        function renderSelection() {
            const lessons = selectedLessons();
            const summary = totals(lessons);

            if (selectedCount) {
                selectedCount.textContent = String(lessons.length);
            }

            if (selectedList) {
                selectedList.innerHTML = lessons.map(lesson => `
                    <li class="selected-lesson-item">
                        <div>
                            <strong>${escapeHtml(lesson.title)}</strong>
                            <small>${lesson.vocabulary} từ vựng · ${lesson.quiz} quiz · ${lesson.duration} phút</small>
                        </div>
                        <div class="selected-lesson-item__actions">
                            <button type="button" class="btn-link-teacher" data-lesson-preview-url="${escapeAttribute(lesson.previewUrl)}">Xem lại</button>
                            <button type="button" class="btn-link-teacher text-danger" data-remove-selected-lesson="${escapeAttribute(lesson.id)}">Bỏ chọn</button>
                        </div>
                    </li>`).join("");
            }

            if (emptySelection) {
                emptySelection.hidden = lessons.length > 0;
            }

            setText("selectedVocabularyTotal", summary.vocabulary);
            setText("selectedQuizTotal", summary.quiz);
            setText("selectedGameTotal", summary.games);
            setText("selectedDurationTotal", summary.duration);
            setText("selectedXpTotal", summary.xp);

            if (selectAll) {
                selectAll.checked = lessonCheckboxes.length > 0 && lessons.length === lessonCheckboxes.length;
                selectAll.indeterminate = lessons.length > 0 && lessons.length < lessonCheckboxes.length;
            }

            if (submitButton) {
                submitButton.disabled = lessons.length === 0;
            }
        }

        function updateStatusPresentation() {
            if (!statusSelect || !submitButton) {
                return;
            }

            const isDraft = statusSelect.value === "Draft";
            submitButton.textContent = isDraft ? "Lưu bản nháp" : "Xuất bản và thông báo";
            if (statusHelp) {
                statusHelp.textContent = isDraft
                    ? "Bản nháp không hiển thị cho học sinh và không gửi thông báo."
                    : "Học sinh đang hoạt động sẽ nhận thông báo sau khi xuất bản.";
            }
        }

        function fillConfirmation() {
            const lessons = selectedLessons();
            const summary = totals(lessons);
            const list = document.getElementById("confirmationLessonList");
            const startDate = form.querySelector('[name="WeekStartDate"]')?.value;
            const dueDate = form.querySelector('[name="DueDate"]')?.value;

            setText("confirmationLessonCount", lessons.length);
            setText("confirmationStudentCount", form.dataset.studentCount || 0);
            setText("confirmationClassName", form.dataset.className || "");
            setText("confirmationPeriod", `${formatDate(startDate)}–${formatDate(dueDate)}`);
            setText("confirmationVocabularyTotal", summary.vocabulary);
            setText("confirmationQuizTotal", summary.quiz);
            setText("confirmationGameTotal", summary.games);
            setText("confirmationDurationTotal", summary.duration);
            setText("confirmationXpTotal", summary.xp);

            if (list) {
                list.innerHTML = lessons.map(lesson => `<li>${escapeHtml(lesson.title)}</li>`).join("");
            }
        }

        lessonCheckboxes.forEach(checkbox => checkbox.addEventListener("change", renderSelection));
        form.querySelectorAll(".content-item-check").forEach(checkbox =>
            checkbox.addEventListener("change", renderSelection));

        form.querySelectorAll(".activity-type-check").forEach(checkbox => {
            const updateGroup = function () {
                const target = document.getElementById(checkbox.dataset.target || "");
                target?.classList.toggle("is-disabled", !checkbox.checked);
                renderSelection();
            };

            checkbox.addEventListener("change", updateGroup);
            updateGroup();
        });
        statusSelect?.addEventListener("change", updateStatusPresentation);

        selectAll?.addEventListener("change", function () {
            lessonCheckboxes.forEach(checkbox => {
                checkbox.checked = selectAll.checked;
            });
            renderSelection();
        });

        document.addEventListener("click", function (event) {
            const toggleButton = event.target.closest("[data-toggle-content-picker]");
            if (toggleButton) {
                const picker = form.querySelector(
                    `[data-content-picker="${toggleButton.dataset.toggleContentPicker}"]`);
                picker?.classList.toggle("is-open");
                toggleButton.textContent = picker?.classList.contains("is-open")
                    ? "Ẩn tùy chọn"
                    : "Tùy chọn nội dung";
                return;
            }

            const removeButton = event.target.closest("[data-remove-selected-lesson]");
            if (!removeButton) {
                return;
            }

            const checkbox = lessonCheckboxes.find(item => item.value === removeButton.dataset.removeSelectedLesson);
            if (checkbox) {
                checkbox.checked = false;
                checkbox.dispatchEvent(new Event("change", { bubbles: true }));
            }
        });

        form.addEventListener("submit", function (event) {
            const invalidLesson = selectedLessons().find(lesson =>
                lesson.vocabulary + lesson.quiz + lesson.games === 0);
            if (invalidLesson) {
                event.preventDefault();
                const picker = form.querySelector(`[data-content-picker="${invalidLesson.id}"]`);
                picker?.classList.add("is-open");
                window.alert(`Vui lòng chọn ít nhất một nội dung cho bài "${invalidLesson.title}".`);
                return;
            }

            if (statusSelect?.value !== "Published" || publishConfirmed) {
                return;
            }

            event.preventDefault();
            fillConfirmation();
            confirmationModal?.show();
        });

        document.getElementById("confirmPublishAssignment")?.addEventListener("click", function () {
            publishConfirmed = true;
            confirmationModal?.hide();
            form.requestSubmit();
        });

        updateStatusPresentation();
        renderSelection();
    });

    function formatDate(value) {
        if (!value) {
            return "Chưa chọn";
        }

        const parts = value.split("-");
        return parts.length === 3 ? `${parts[2]}/${parts[1]}/${parts[0]}` : value;
    }

    function escapeHtml(value) {
        const element = document.createElement("div");
        element.textContent = value;
        return element.innerHTML;
    }

    function escapeAttribute(value) {
        return escapeHtml(value).replace(/"/g, "&quot;");
    }
})();
