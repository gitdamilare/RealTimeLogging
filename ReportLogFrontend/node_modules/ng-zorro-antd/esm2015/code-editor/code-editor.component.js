/**
 * Use of this source code is governed by an MIT-style license that can be
 * found in the LICENSE file at https://github.com/NG-ZORRO/ng-zorro-antd/blob/master/LICENSE
 */
import { __decorate, __metadata } from "tslib";
import { Platform } from '@angular/cdk/platform';
import { ChangeDetectionStrategy, Component, ElementRef, EventEmitter, forwardRef, Input, NgZone, Output, TemplateRef, ViewEncapsulation } from '@angular/core';
import { NG_VALUE_ACCESSOR } from '@angular/forms';
import { warn } from 'ng-zorro-antd/core/logger';
import { inNextTick, InputBoolean } from 'ng-zorro-antd/core/util';
import { BehaviorSubject, combineLatest, fromEvent, Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged, filter, map, takeUntil } from 'rxjs/operators';
import { NzCodeEditorService } from './code-editor.service';
export class NzCodeEditorComponent {
    constructor(nzCodeEditorService, ngZone, elementRef, platform) {
        this.nzCodeEditorService = nzCodeEditorService;
        this.ngZone = ngZone;
        this.platform = platform;
        this.nzEditorMode = 'normal';
        this.nzOriginalText = '';
        this.nzLoading = false;
        this.nzFullControl = false;
        this.nzEditorInitialized = new EventEmitter();
        this.editorOptionCached = {};
        this.destroy$ = new Subject();
        this.resize$ = new Subject();
        this.editorOption$ = new BehaviorSubject({});
        this.value = '';
        this.modelSet = false;
        this.onChange = (_value) => { };
        this.onTouch = () => { };
        this.el = elementRef.nativeElement;
    }
    set nzEditorOption(value) {
        this.editorOption$.next(value);
    }
    /**
     * Initialize a monaco editor instance.
     */
    ngAfterViewInit() {
        if (!this.platform.isBrowser) {
            return;
        }
        this.nzCodeEditorService.requestToInit().subscribe(option => this.setup(option));
    }
    ngOnDestroy() {
        if (this.editorInstance) {
            this.editorInstance.dispose();
        }
        this.destroy$.next();
        this.destroy$.complete();
    }
    writeValue(value) {
        this.value = value;
        this.setValue();
    }
    registerOnChange(fn) {
        this.onChange = fn;
    }
    registerOnTouched(fn) {
        this.onTouch = fn;
    }
    layout() {
        this.resize$.next();
    }
    setup(option) {
        inNextTick().subscribe(() => {
            this.editorOptionCached = option;
            this.registerOptionChanges();
            this.initMonacoEditorInstance();
            this.registerResizeChange();
            this.setValue();
            if (!this.nzFullControl) {
                this.setValueEmitter();
            }
            this.nzEditorInitialized.emit(this.editorInstance);
        });
    }
    registerOptionChanges() {
        combineLatest([this.editorOption$, this.nzCodeEditorService.option$])
            .pipe(takeUntil(this.destroy$))
            .subscribe(([selfOpt, defaultOpt]) => {
            this.editorOptionCached = Object.assign(Object.assign(Object.assign({}, this.editorOptionCached), defaultOpt), selfOpt);
            this.updateOptionToMonaco();
        });
    }
    initMonacoEditorInstance() {
        this.ngZone.runOutsideAngular(() => {
            this.editorInstance =
                this.nzEditorMode === 'normal'
                    ? monaco.editor.create(this.el, Object.assign({}, this.editorOptionCached))
                    : monaco.editor.createDiffEditor(this.el, Object.assign({}, this.editorOptionCached));
        });
    }
    registerResizeChange() {
        this.ngZone.runOutsideAngular(() => {
            fromEvent(window, 'resize')
                .pipe(debounceTime(300), takeUntil(this.destroy$))
                .subscribe(() => {
                this.layout();
            });
            this.resize$
                .pipe(takeUntil(this.destroy$), filter(() => !!this.editorInstance), map(() => ({
                width: this.el.clientWidth,
                height: this.el.clientHeight
            })), distinctUntilChanged((a, b) => a.width === b.width && a.height === b.height), debounceTime(50))
                .subscribe(() => {
                this.editorInstance.layout();
            });
        });
    }
    setValue() {
        if (!this.editorInstance) {
            return;
        }
        if (this.nzFullControl && this.value) {
            warn(`should not set value when you are using full control mode! It would result in ambiguous data flow!`);
            return;
        }
        if (this.nzEditorMode === 'normal') {
            if (this.modelSet) {
                const model = this.editorInstance.getModel();
                this.preservePositionAndSelections(() => model.setValue(this.value));
            }
            else {
                this.editorInstance.setModel(monaco.editor.createModel(this.value, this.editorOptionCached.language));
                this.modelSet = true;
            }
        }
        else {
            if (this.modelSet) {
                const model = this.editorInstance.getModel();
                this.preservePositionAndSelections(() => {
                    model.modified.setValue(this.value);
                    model.original.setValue(this.nzOriginalText);
                });
            }
            else {
                const language = this.editorOptionCached.language;
                this.editorInstance.setModel({
                    original: monaco.editor.createModel(this.nzOriginalText, language),
                    modified: monaco.editor.createModel(this.value, language)
                });
                this.modelSet = true;
            }
        }
    }
    /**
     * {@link editor.ICodeEditor}#setValue resets the cursor position to the start of the document.
     * This helper memorizes the cursor position and selections and restores them after the given
     * function has been called.
     */
    preservePositionAndSelections(fn) {
        if (!this.editorInstance) {
            fn();
            return;
        }
        const position = this.editorInstance.getPosition();
        const selections = this.editorInstance.getSelections();
        fn();
        if (position) {
            this.editorInstance.setPosition(position);
        }
        if (selections) {
            this.editorInstance.setSelections(selections);
        }
    }
    setValueEmitter() {
        const model = (this.nzEditorMode === 'normal'
            ? this.editorInstance.getModel()
            : this.editorInstance.getModel().modified);
        model.onDidChangeContent(() => {
            this.ngZone.run(() => {
                this.emitValue(model.getValue());
            });
        });
    }
    emitValue(value) {
        if (this.value === value) {
            // If the value didn't change there's no reason to send an update.
            // Specifically this may happen during an update from the model (writeValue) where sending an update to the model would actually be incorrect.
            return;
        }
        this.value = value;
        this.onChange(value);
    }
    updateOptionToMonaco() {
        if (this.editorInstance) {
            this.editorInstance.updateOptions(Object.assign({}, this.editorOptionCached));
        }
    }
}
NzCodeEditorComponent.decorators = [
    { type: Component, args: [{
                changeDetection: ChangeDetectionStrategy.OnPush,
                encapsulation: ViewEncapsulation.None,
                selector: 'nz-code-editor',
                exportAs: 'nzCodeEditor',
                template: `
    <div class="ant-code-editor-loading" *ngIf="nzLoading">
      <nz-spin></nz-spin>
    </div>

    <div class="ant-code-editor-toolkit" *ngIf="nzToolkit">
      <ng-template [ngTemplateOutlet]="nzToolkit"></ng-template>
    </div>
  `,
                host: {
                    '[class.ant-code-editor]': 'true'
                },
                providers: [
                    {
                        provide: NG_VALUE_ACCESSOR,
                        useExisting: forwardRef(() => NzCodeEditorComponent),
                        multi: true
                    }
                ]
            },] }
];
NzCodeEditorComponent.ctorParameters = () => [
    { type: NzCodeEditorService },
    { type: NgZone },
    { type: ElementRef },
    { type: Platform }
];
NzCodeEditorComponent.propDecorators = {
    nzEditorMode: [{ type: Input }],
    nzOriginalText: [{ type: Input }],
    nzLoading: [{ type: Input }],
    nzFullControl: [{ type: Input }],
    nzToolkit: [{ type: Input }],
    nzEditorOption: [{ type: Input }],
    nzEditorInitialized: [{ type: Output }]
};
__decorate([
    InputBoolean(),
    __metadata("design:type", Object)
], NzCodeEditorComponent.prototype, "nzLoading", void 0);
__decorate([
    InputBoolean(),
    __metadata("design:type", Object)
], NzCodeEditorComponent.prototype, "nzFullControl", void 0);
//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJmaWxlIjoiY29kZS1lZGl0b3IuY29tcG9uZW50LmpzIiwic291cmNlUm9vdCI6Ii9ob21lL3ZzdHMvd29yay8xL3MvY29tcG9uZW50cy9jb2RlLWVkaXRvci8iLCJzb3VyY2VzIjpbImNvZGUtZWRpdG9yLmNvbXBvbmVudC50cyJdLCJuYW1lcyI6W10sIm1hcHBpbmdzIjoiQUFBQTs7O0dBR0c7O0FBRUgsT0FBTyxFQUFFLFFBQVEsRUFBRSxNQUFNLHVCQUF1QixDQUFDO0FBQ2pELE9BQU8sRUFFTCx1QkFBdUIsRUFDdkIsU0FBUyxFQUNULFVBQVUsRUFDVixZQUFZLEVBQ1osVUFBVSxFQUNWLEtBQUssRUFDTCxNQUFNLEVBRU4sTUFBTSxFQUNOLFdBQVcsRUFDWCxpQkFBaUIsRUFDbEIsTUFBTSxlQUFlLENBQUM7QUFDdkIsT0FBTyxFQUFFLGlCQUFpQixFQUFFLE1BQU0sZ0JBQWdCLENBQUM7QUFHbkQsT0FBTyxFQUFFLElBQUksRUFBRSxNQUFNLDJCQUEyQixDQUFDO0FBRWpELE9BQU8sRUFBRSxVQUFVLEVBQUUsWUFBWSxFQUFFLE1BQU0seUJBQXlCLENBQUM7QUFDbkUsT0FBTyxFQUFFLGVBQWUsRUFBRSxhQUFhLEVBQUUsU0FBUyxFQUFFLE9BQU8sRUFBRSxNQUFNLE1BQU0sQ0FBQztBQUMxRSxPQUFPLEVBQUUsWUFBWSxFQUFFLG9CQUFvQixFQUFFLE1BQU0sRUFBRSxHQUFHLEVBQUUsU0FBUyxFQUFFLE1BQU0sZ0JBQWdCLENBQUM7QUFFNUYsT0FBTyxFQUFFLG1CQUFtQixFQUFFLE1BQU0sdUJBQXVCLENBQUM7QUFpQzVELE1BQU0sT0FBTyxxQkFBcUI7SUEwQmhDLFlBQ1UsbUJBQXdDLEVBQ3hDLE1BQWMsRUFDdEIsVUFBc0IsRUFDZCxRQUFrQjtRQUhsQix3QkFBbUIsR0FBbkIsbUJBQW1CLENBQXFCO1FBQ3hDLFdBQU0sR0FBTixNQUFNLENBQVE7UUFFZCxhQUFRLEdBQVIsUUFBUSxDQUFVO1FBMUJuQixpQkFBWSxHQUFpQixRQUFRLENBQUM7UUFDdEMsbUJB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QixDQUFDLE1BQU07Z0JBQy9DLGFBQWEsRUFBRSxpQkFBaUIsQ0FBQyxJQUFJO2dCQUNyQyxRQUFRLEVBQUUsZ0JBQWdCO2dCQUMxQixRQUFRLEVBQUUsY0FBYztnQkFDeEIsUUFBUSxFQUFFOzs7Ozs7OztHQVFUO2dCQUNELElBQUksRUFBRTtvQkFDSix5QkFBeUIsRUFBRSxNQUFNO2lCQUNsQztnQkFDRCxTQUFTLEVBQUU7b0JBQ1Q7d0JBQ0UsT0FBTyxFQUFFLGlCQUFpQjt3QkFDMUIsV0FBVyxFQUFFLFVBQVUsQ0FBQyxHQUFHLEVBQUUsQ0FBQyxxQkFBcUIsQ0FBQzt3QkFDcEQsS0FBSyxFQUFFLElBQUk7cUJBQ1o7aUJBQ0Y7YUFDRjs7O1lBaENRLG1CQUFtQjtZQWYxQixNQUFNO1lBSk4sVUFBVTtZQUxILFFBQVE7OzsyQkE2RGQsS0FBSzs2QkFDTCxLQUFLO3dCQUNMLEtBQUs7NEJBQ0wsS0FBSzt3QkFDTCxLQUFLOzZCQUVMLEtBQUs7a0NBSUwsTUFBTTs7QUFSa0I7SUFBZixZQUFZLEVBQUU7O3dEQUFtQjtBQUNsQjtJQUFmLFlBQVksRUFBRTs7NERBQXVCIiwic291cmNlc0NvbnRlbnQiOlsiLyoqXG4gKiBVc2Ugb2YgdGhpcyBzb3VyY2UgY29kZSBpcyBnb3Zlcm5lZCBieSBhbiBNSVQtc3R5bGUgbGljZW5zZSB0aGF0IGNhbiBiZVxuICogZm91bmQgaW4gdGhlIExJQ0VOU0UgZmlsZSBhdCBodHRwczovL2dpdGh1Yi5jb20vTkctWk9SUk8vbmctem9ycm8tYW50ZC9ibG9iL21hc3Rlci9MSUNFTlNFXG4gKi9cblxuaW1wb3J0IHsgUGxhdGZvcm0gfSBmcm9tICdAYW5ndWxhci9jZGsvcGxhdGZvcm0nO1xuaW1wb3J0IHtcbiAgQWZ0ZXJWaWV3SW5pdCxcbiAgQ2hhbmdlRGV0ZWN0aW9uU3RyYXRlZ3ksXG4gIENvbXBvbmVudCxcbiAgRWxlbWVudFJlZixcbiAgRXZlbnRFbWl0dGVyLFxuICBmb3J3YXJkUmVmLFxuICBJbnB1dCxcbiAgTmdab25lLFxuICBPbkRlc3Ryb3ksXG4gIE91dHB1dCxcbiAgVGVtcGxhdGVSZWYsXG4gIFZpZXdFbmNhcHN1bGF0aW9uXG59IGZyb20gJ0Bhbmd1bGFyL2NvcmUnO1xuaW1wb3J0IHsgTkdfVkFMVUVfQUNDRVNTT1IgfSBmcm9tICdAYW5ndWxhci9mb3Jtcyc7XG4vLyBJbXBvcnQgdHlwZXMgZnJvbSBtb25hY28gZWRpdG9yLlxuaW1wb3J0IHsgZWRpdG9yIH0gZnJvbSAnbW9uYWNvLWVkaXRvcic7XG5pbXBvcnQgeyB3YXJuIH0gZnJvbSAnbmctem9ycm8tYW50ZC9jb3JlL2xvZ2dlcic7XG5pbXBvcnQgeyBCb29sZWFuSW5wdXQsIE56U2FmZUFueSwgT25DaGFuZ2VUeXBlLCBPblRvdWNoZWRUeXBlIH0gZnJvbSAnbmctem9ycm8tYW50ZC9jb3JlL3R5cGVzJztcbmltcG9ydCB7IGluTmV4dFRpY2ssIElucHV0Qm9vbGVhbiB9IGZyb20gJ25nLXpvcnJvLWFudGQvY29yZS91dGlsJztcbmltcG9ydCB7IEJlaGF2aW9yU3ViamVjdCwgY29tYmluZUxhdGVzdCwgZnJvbUV2ZW50LCBTdWJqZWN0IH0gZnJvbSAncnhqcyc7XG5pbXBvcnQgeyBkZWJvdW5jZVRpbWUsIGRpc3RpbmN0VW50aWxDaGFuZ2VkLCBmaWx0ZXIsIG1hcCwgdGFrZVVudGlsIH0gZnJvbSAncnhqcy9vcGVyYXRvcnMnO1xuXG5pbXBvcnQgeyBOekNvZGVFZGl0b3JTZXJ2aWNlIH0gZnJvbSAnLi9jb2RlLWVkaXRvci5zZXJ2aWNlJztcbmltcG9ydCB7IERpZmZFZGl0b3JPcHRpb25zLCBFZGl0b3JPcHRpb25zLCBKb2luZWRFZGl0b3JPcHRpb25zLCBOekVkaXRvck1vZGUgfSBmcm9tICcuL3R5cGluZ3MnO1xuaW1wb3J0IElUZXh0TW9kZWwgPSBlZGl0b3IuSVRleHRNb2RlbDtcbmltcG9ydCBJU3RhbmRhbG9uZUNvZGVFZGl0b3IgPSBlZGl0b3IuSVN0YW5kYWxvbmVDb2RlRWRpdG9yO1xuaW1wb3J0IElTdGFuZGFsb25lRGlmZkVkaXRvciA9IGVkaXRvci5JU3RhbmRhbG9uZURpZmZFZGl0b3I7XG5cbmRlY2xhcmUgY29uc3QgbW9uYWNvOiBOelNhZmVBbnk7XG5cbkBDb21wb25lbnQoe1xuICBjaGFuZ2VEZXRlY3Rpb246IENoYW5nZURldGVjdGlvblN0cmF0ZWd5Lk9uUHVzaCxcbiAgZW5jYXBzdWxhdGlvbjogVmlld0VuY2Fwc3VsYXRpb24uTm9uZSxcbiAgc2VsZWN0b3I6ICduei1jb2RlLWVkaXRvcicsXG4gIGV4cG9ydEFzOiAnbnpDb2RlRWRpdG9yJyxcbiAgdGVtcGxhdGU6IGBcbiAgICA8ZGl2IGNsYXNzPVwiYW50LWNvZGUtZWRpdG9yLWxvYWRpbmdcIiAqbmdJZj1cIm56TG9hZGluZ1wiPlxuICAgICAgPG56LXNwaW4+PC9uei1zcGluPlxuICAgIDwvZGl2PlxuXG4gICAgPGRpdiBjbGFzcz1cImFudC1jb2RlLWVkaXRvci10b29sa2l0XCIgKm5nSWY9XCJuelRvb2xraXRcIj5cbiAgICAgIDxuZy10ZW1wbGF0ZSBbbmdUZW1wbGF0ZU91dGxldF09XCJuelRvb2xraXRcIj48L25nLXRlbXBsYXRlPlxuICAgIDwvZGl2PlxuICBgLFxuICBob3N0OiB7XG4gICAgJ1tjbGFzcy5hbnQtY29kZS1lZGl0b3JdJzogJ3RydWUnXG4gIH0sXG4gIHByb3ZpZGVyczogW1xuICAgIHtcbiAgICAgIHByb3ZpZGU6IE5HX1ZBTFVFX0FDQ0VTU09SLFxuICAgICAgdXNlRXhpc3Rpbmc6IGZvcndhcmRSZWYoKCkgPT4gTnpDb2RlRWRpdG9yQ29tcG9uZW50KSxcbiAgICAgIG11bHRpOiB0cnVlXG4gICAgfVxuICBdXG59KVxuZXhwb3J0IGNsYXNzIE56Q29kZUVkaXRvckNvbXBvbmVudCBpbXBsZW1lbnRzIE9uRGVzdHJveSwgQWZ0ZXJWaWV3SW5pdCB7XG4gIHN0YXRpYyBuZ0FjY2VwdElucHV0VHlwZV9uekxvYWRpbmc6IEJvb2xlYW5JbnB1dDtcbiAgc3RhdGljIG5nQWNjZXB0SW5wdXRUeXBlX256RnVsbENvbnRyb2w6IEJvb2xlYW5JbnB1dDtcblxuICBASW5wdXQoKSBuekVkaXRvck1vZGU6IE56RWRpdG9yTW9kZSA9ICdub3JtYWwnO1xuICBASW5wdXQoKSBuek9yaWdpbmFsVGV4dCA9ICcnO1xuICBASW5wdXQoKSBASW5wdXRCb29sZWFuKCkgbnpMb2FkaW5nID0gZmFsc2U7XG4gIEBJbnB1dCgpIEBJbnB1dEJvb2xlYW4oKSBuekZ1bGxDb250cm9sID0gZmFsc2U7XG4gIEBJbnB1dCgpIG56VG9vbGtpdD86IFRlbXBsYXRlUmVmPHZvaWQ+O1xuXG4gIEBJbnB1dCgpIHNldCBuekVkaXRvck9wdGlvbih2YWx1ZTogSm9pbmVkRWRpdG9yT3B0aW9ucykge1xuICAgIHRoaXMuZWRpdG9yT3B0aW9uJC5uZXh0KHZhbHVlKTtcbiAgfVxuXG4gIEBPdXRwdXQoKSByZWFkb25seSBuekVkaXRvckluaXRpYWxpemVkID0gbmV3IEV2ZW50RW1pdHRlcjxJU3RhbmRhbG9uZUNvZGVFZGl0b3IgfCBJU3RhbmRhbG9uZURpZmZFZGl0b3I+KCk7XG5cbiAgZWRpdG9yT3B0aW9uQ2FjaGVkOiBKb2luZWRFZGl0b3JPcHRpb25zID0ge307XG5cbiAgcHJpdmF0ZSByZWFkb25seSBlbDogSFRNTEVsZW1lbnQ7XG4gIHByaXZhdGUgZGVzdHJveSQgPSBuZXcgU3ViamVjdDx2b2lkPigpO1xuICBwcml2YXRlIHJlc2l6ZSQgPSBuZXcgU3ViamVjdDx2b2lkPigpO1xuICBwcml2YXRlIGVkaXRvck9wdGlvbiQgPSBuZXcgQmVoYXZpb3JTdWJqZWN0PEpvaW5lZEVkaXRvck9wdGlvbnM+KHt9KTtcbiAgcHJpdmF0ZSBlZGl0b3JJbnN0YW5jZT86IElTdGFuZGFsb25lQ29kZUVkaXRvciB8IElTdGFuZGFsb25lRGlmZkVkaXRvcjtcbiAgcHJpdmF0ZSB2YWx1ZSA9ICcnO1xuICBwcml2YXRlIG1vZGVsU2V0ID0gZmFsc2U7XG5cbiAgY29uc3RydWN0b3IoXG4gICAgcHJpdmF0ZSBuekNvZGVFZGl0b3JTZXJ2aWNlOiBOekNvZGVFZGl0b3JTZXJ2aWNlLFxuICAgIHByaXZhdGUgbmdab25lOiBOZ1pvbmUsXG4gICAgZWxlbWVudFJlZjogRWxlbWVudFJlZixcbiAgICBwcml2YXRlIHBsYXRmb3JtOiBQbGF0Zm9ybVxuICApIHtcbiAgICB0aGlzLmVsID0gZWxlbWVudFJlZi5uYXRpdmVFbGVtZW50O1xuICB9XG5cbiAgLyoqXG4gICAqIEluaXRpYWxpemUgYSBtb25hY28gZWRpdG9yIGluc3RhbmNlLlxuICAgKi9cbiAgbmdBZnRlclZpZXdJbml0KCk6IHZvaWQge1xuICAgIGlmICghdGhpcy5wbGF0Zm9ybS5pc0Jyb3dzZXIpIHtcbiAgICAgIHJldHVybjtcbiAgICB9XG4gICAgdGhpcy5uekNvZGVFZGl0b3JTZXJ2aWNlLnJlcXVlc3RUb0luaXQoKS5zdWJzY3JpYmUob3B0aW9uID0+IHRoaXMuc2V0dXAob3B0aW9uKSk7XG4gIH1cblxuICBuZ09uRGVzdHJveSgpOiB2b2lkIHtcbiAgICBpZiAodGhpcy5lZGl0b3JJbnN0YW5jZSkge1xuICAgICAgdGhpcy5lZGl0b3JJbnN0YW5jZS5kaXNwb3NlKCk7XG4gICAgfVxuXG4gICAgdGhpcy5kZXN0cm95JC5uZXh0KCk7XG4gICAgdGhpcy5kZXN0cm95JC5jb21wbGV0ZSgpO1xuICB9XG5cbiAgd3JpdGVWYWx1ZSh2YWx1ZTogc3RyaW5nKTogdm9pZCB7XG4gICAgdGhpcy52YWx1ZSA9IHZhbHVlO1xuICAgIHRoaXMuc2V0VmFsdWUoKTtcbiAgfVxuXG4gIHJlZ2lzdGVyT25DaGFuZ2UoZm46IE9uQ2hhbmdlVHlwZSk6IE56U2FmZUFueSB7XG4gICAgdGhpcy5vbkNoYW5nZSA9IGZuO1xuICB9XG5cbiAgcmVnaXN0ZXJPblRvdWNoZWQoZm46IE9uVG91Y2hlZFR5cGUpOiB2b2lkIHtcbiAgICB0aGlzLm9uVG91Y2ggPSBmbjtcbiAgfVxuXG4gIG9uQ2hhbmdlOiBPbkNoYW5nZVR5cGUgPSAoX3ZhbHVlOiBzdHJpbmcpID0+IHt9O1xuXG4gIG9uVG91Y2g6IE9uVG91Y2hlZFR5cGUgPSAoKSA9PiB7fTtcblxuICBsYXlvdXQoKTogdm9pZCB7XG4gICAgdGhpcy5yZXNpemUkLm5leHQoKTtcbiAgfVxuXG4gIHByaXZhdGUgc2V0dXAob3B0aW9uOiBKb2luZWRFZGl0b3JPcHRpb25zKTogdm9pZCB7XG4gICAgaW5OZXh0VGljaygpLnN1YnNjcmliZSgoKSA9PiB7XG4gICAgICB0aGlzLmVkaXRvck9wdGlvbkNhY2hlZCA9IG9wdGlvbjtcbiAgICAgIHRoaXMucmVnaXN0ZXJPcHRpb25DaGFuZ2VzKCk7XG4gICAgICB0aGlzLmluaXRNb25hY29FZGl0b3JJbnN0YW5jZSgpO1xuICAgICAgdGhpcy5yZWdpc3RlclJlc2l6ZUNoYW5nZSgpO1xuICAgICAgdGhpcy5zZXRWYWx1ZSgpO1xuXG4gICAgICBpZiAoIXRoaXMubnpGdWxsQ29udHJvbCkge1xuICAgICAgICB0aGlzLnNldFZhbHVlRW1pdHRlcigpO1xuICAgICAgfVxuXG4gICAgICB0aGlzLm56RWRpdG9ySW5pdGlhbGl6ZWQuZW1pdCh0aGlzLmVkaXRvckluc3RhbmNlKTtcbiAgICB9KTtcbiAgfVxuXG4gIHByaXZhdGUgcmVnaXN0ZXJPcHRpb25DaGFuZ2VzKCk6IHZvaWQge1xuICAgIGNvbWJpbmVMYXRlc3QoW3RoaXMuZWRpdG9yT3B0aW9uJCwgdGhpcy5uekNvZGVFZGl0b3JTZXJ2aWNlLm9wdGlvbiRdKVxuICAgICAgLnBpcGUodGFrZVVudGlsKHRoaXMuZGVzdHJveSQpKVxuICAgICAgLnN1YnNjcmliZSgoW3NlbGZPcHQsIGRlZmF1bHRPcHRdKSA9PiB7XG4gICAgICAgIHRoaXMuZWRpdG9yT3B0aW9uQ2FjaGVkID0ge1xuICAgICAgICAgIC4uLnRoaXMuZWRpdG9yT3B0aW9uQ2FjaGVkLFxuICAgICAgICAgIC4uLmRlZmF1bHRPcHQsXG4gICAgICAgICAgLi4uc2VsZk9wdFxuICAgICAgICB9O1xuICAgICAgICB0aGlzLnVwZGF0ZU9wdGlvblRvTW9uYWNvKCk7XG4gICAgICB9KTtcbiAgfVxuXG4gIHByaXZhdGUgaW5pdE1vbmFjb0VkaXRvckluc3RhbmNlKCk6IHZvaWQge1xuICAgIHRoaXMubmdab25lLnJ1bk91dHNpZGVBbmd1bGFyKCgpID0+IHtcbiAgICAgIHRoaXMuZWRpdG9ySW5zdGFuY2UgPVxuICAgICAgICB0aGlzLm56RWRpdG9yTW9kZSA9PT0gJ25vcm1hbCdcbiAgICAgICAgICA/IG1vbmFjby5lZGl0b3IuY3JlYXRlKHRoaXMuZWwsIHsgLi4udGhpcy5lZGl0b3JPcHRpb25DYWNoZWQgfSlcbiAgICAgICAgICA6IG1vbmFjby5lZGl0b3IuY3JlYXRlRGlmZkVkaXRvcih0aGlzLmVsLCB7XG4gICAgICAgICAgICAgIC4uLih0aGlzLmVkaXRvck9wdGlvbkNhY2hlZCBhcyBEaWZmRWRpdG9yT3B0aW9ucylcbiAgICAgICAgICAgIH0pO1xuICAgIH0pO1xuICB9XG5cbiAgcHJpdmF0ZSByZWdpc3RlclJlc2l6ZUNoYW5nZSgpOiB2b2lkIHtcbiAgICB0aGlzLm5nWm9uZS5ydW5PdXRzaWRlQW5ndWxhcigoKSA9PiB7XG4gICAgICBmcm9tRXZlbnQod2luZG93LCAncmVzaXplJylcbiAgICAgICAgLnBpcGUoZGVib3VuY2VUaW1lKDMwMCksIHRha2VVbnRpbCh0aGlzLmRlc3Ryb3kkKSlcbiAgICAgICAgLnN1YnNjcmliZSgoKSA9PiB7XG4gICAgICAgICAgdGhpcy5sYXlvdXQoKTtcbiAgICAgICAgfSk7XG5cbiAgICAgIHRoaXMucmVzaXplJFxuICAgICAgICAucGlwZShcbiAgICAgICAgICB0YWtlVW50aWwodGhpcy5kZXN0cm95JCksXG4gICAgICAgICAgZmlsdGVyKCgpID0+ICEhdGhpcy5lZGl0b3JJbnN0YW5jZSksXG4gICAgICAgICAgbWFwKCgpID0+ICh7XG4gICAgICAgICAgICB3aWR0aDogdGhpcy5lbC5jbGllbnRXaWR0aCxcbiAgICAgICAgICAgIGhlaWdodDogdGhpcy5lbC5jbGllbnRIZWlnaHRcbiAgICAgICAgICB9KSksXG4gICAgICAgICAgZGlzdGluY3RVbnRpbENoYW5nZWQoKGEsIGIpID0+IGEud2lkdGggPT09IGIud2lkdGggJiYgYS5oZWlnaHQgPT09IGIuaGVpZ2h0KSxcbiAgICAgICAgICBkZWJvdW5jZVRpbWUoNTApXG4gICAgICAgIClcbiAgICAgICAgLnN1YnNjcmliZSgoKSA9PiB7XG4gICAgICAgICAgdGhpcy5lZGl0b3JJbnN0YW5jZSEubGF5b3V0KCk7XG4gICAgICAgIH0pO1xuICAgIH0pO1xuICB9XG5cbiAgcHJpdmF0ZSBzZXRWYWx1ZSgpOiB2b2lkIHtcbiAgICBpZiAoIXRoaXMuZWRpdG9ySW5zdGFuY2UpIHtcbiAgICAgIHJldHVybjtcbiAgICB9XG5cbiAgICBpZiAodGhpcy5uekZ1bGxDb250cm9sICYmIHRoaXMudmFsdWUpIHtcbiAgICAgIHdhcm4oYHNob3VsZCBub3Qgc2V0IHZhbHVlIHdoZW4geW91IGFyZSB1c2luZyBmdWxsIGNvbnRyb2wgbW9kZSEgSXQgd291bGQgcmVzdWx0IGluIGFtYmlndW91cyBkYXRhIGZsb3chYCk7XG4gICAgICByZXR1cm47XG4gICAgfVxuXG4gICAgaWYgKHRoaXMubnpFZGl0b3JNb2RlID09PSAnbm9ybWFsJykge1xuICAgICAgaWYgKHRoaXMubW9kZWxTZXQpIHtcbiAgICAgICAgY29uc3QgbW9kZWwgPSB0aGlzLmVkaXRvckluc3RhbmNlLmdldE1vZGVsKCkgYXMgSVRleHRNb2RlbDtcbiAgICAgICAgdGhpcy5wcmVzZXJ2ZVBvc2l0aW9uQW5kU2VsZWN0aW9ucygoKSA9PiBtb2RlbC5zZXRWYWx1ZSh0aGlzLnZhbHVlKSk7XG4gICAgICB9IGVsc2Uge1xuICAgICAgICAodGhpcy5lZGl0b3JJbnN0YW5jZSBhcyBJU3RhbmRhbG9uZUNvZGVFZGl0b3IpLnNldE1vZGVsKFxuICAgICAgICAgIG1vbmFjby5lZGl0b3IuY3JlYXRlTW9kZWwodGhpcy52YWx1ZSwgKHRoaXMuZWRpdG9yT3B0aW9uQ2FjaGVkIGFzIEVkaXRvck9wdGlvbnMpLmxhbmd1YWdlKVxuICAgICAgICApO1xuICAgICAgICB0aGlzLm1vZGVsU2V0ID0gdHJ1ZTtcbiAgICAgIH1cbiAgICB9IGVsc2Uge1xuICAgICAgaWYgKHRoaXMubW9kZWxTZXQpIHtcbiAgICAgICAgY29uc3QgbW9kZWwgPSAodGhpcy5lZGl0b3JJbnN0YW5jZSBhcyBJU3RhbmRhbG9uZURpZmZFZGl0b3IpLmdldE1vZGVsKCkhO1xuICAgICAgICB0aGlzLnByZXNlcnZlUG9zaXRpb25BbmRTZWxlY3Rpb25zKCgpID0+IHtcbiAgICAgICAgICBtb2RlbC5tb2RpZmllZC5zZXRWYWx1ZSh0aGlzLnZhbHVlKTtcbiAgICAgICAgICBtb2RlbC5vcmlnaW5hbC5zZXRWYWx1ZSh0aGlzLm56T3JpZ2luYWxUZXh0KTtcbiAgICAgICAgfSk7XG4gICAgICB9IGVsc2Uge1xuICAgICAgICBjb25zdCBsYW5ndWFnZSA9ICh0aGlzLmVkaXRvck9wdGlvbkNhY2hlZCBhcyBFZGl0b3JPcHRpb25zKS5sYW5ndWFnZTtcbiAgICAgICAgKHRoaXMuZWRpdG9ySW5zdGFuY2UgYXMgSVN0YW5kYWxvbmVEaWZmRWRpdG9yKS5zZXRNb2RlbCh7XG4gICAgICAgICAgb3JpZ2luYWw6IG1vbmFjby5lZGl0b3IuY3JlYXRlTW9kZWwodGhpcy5uek9yaWdpbmFsVGV4dCwgbGFuZ3VhZ2UpLFxuICAgICAgICAgIG1vZGlmaWVkOiBtb25hY28uZWRpdG9yLmNyZWF0ZU1vZGVsKHRoaXMudmFsdWUsIGxhbmd1YWdlKVxuICAgICAgICB9KTtcbiAgICAgICAgdGhpcy5tb2RlbFNldCA9IHRydWU7XG4gICAgICB9XG4gICAgfVxuICB9XG5cbiAgLyoqXG4gICAqIHtAbGluayBlZGl0b3IuSUNvZGVFZGl0b3J9I3NldFZhbHVlIHJlc2V0cyB0aGUgY3Vyc29yIHBvc2l0aW9uIHRvIHRoZSBzdGFydCBvZiB0aGUgZG9jdW1lbnQuXG4gICAqIFRoaXMgaGVscGVyIG1lbW9yaXplcyB0aGUgY3Vyc29yIHBvc2l0aW9uIGFuZCBzZWxlY3Rpb25zIGFuZCByZXN0b3JlcyB0aGVtIGFmdGVyIHRoZSBnaXZlblxuICAgKiBmdW5jdGlvbiBoYXMgYmVlbiBjYWxsZWQuXG4gICAqL1xuICBwcml2YXRlIHByZXNlcnZlUG9zaXRpb25BbmRTZWxlY3Rpb25zKGZuOiAoKSA9PiB1bmtub3duKTogdm9pZCB7XG4gICAgaWYgKCF0aGlzLmVkaXRvckluc3RhbmNlKSB7XG4gICAgICBmbigpO1xuICAgICAgcmV0dXJuO1xuICAgIH1cblxuICAgIGNvbnN0IHBvc2l0aW9uID0gdGhpcy5lZGl0b3JJbnN0YW5jZS5nZXRQb3NpdGlvbigpO1xuICAgIGNvbnN0IHNlbGVjdGlvbnMgPSB0aGlzLmVkaXRvckluc3RhbmNlLmdldFNlbGVjdGlvbnMoKTtcblxuICAgIGZuKCk7XG5cbiAgICBpZiAocG9zaXRpb24pIHtcbiAgICAgIHRoaXMuZWRpdG9ySW5zdGFuY2Uuc2V0UG9zaXRpb24ocG9zaXRpb24pO1xuICAgIH1cbiAgICBpZiAoc2VsZWN0aW9ucykge1xuICAgICAgdGhpcy5lZGl0b3JJbnN0YW5jZS5zZXRTZWxlY3Rpb25zKHNlbGVjdGlvbnMpO1xuICAgIH1cbiAgfVxuXG4gIHByaXZhdGUgc2V0VmFsdWVFbWl0dGVyKCk6IHZvaWQge1xuICAgIGNvbnN0IG1vZGVsID0gKHRoaXMubnpFZGl0b3JNb2RlID09PSAnbm9ybWFsJ1xuICAgICAgPyAodGhpcy5lZGl0b3JJbnN0YW5jZSBhcyBJU3RhbmRhbG9uZUNvZGVFZGl0b3IpLmdldE1vZGVsKClcbiAgICAgIDogKHRoaXMuZWRpdG9ySW5zdGFuY2UgYXMgSVN0YW5kYWxvbmVEaWZmRWRpdG9yKS5nZXRNb2RlbCgpIS5tb2RpZmllZCkgYXMgSVRleHRNb2RlbDtcblxuICAgIG1vZGVsLm9uRGlkQ2hhbmdlQ29udGVudCgoKSA9PiB7XG4gICAgICB0aGlzLm5nWm9uZS5ydW4oKCkgPT4ge1xuICAgICAgICB0aGlzLmVtaXRWYWx1ZShtb2RlbC5nZXRWYWx1ZSgpKTtcbiAgICAgIH0pO1xuICAgIH0pO1xuICB9XG5cbiAgcHJpdmF0ZSBlbWl0VmFsdWUodmFsdWU6IHN0cmluZyk6IHZvaWQge1xuICAgIGlmICh0aGlzLnZhbHVlID09PSB2YWx1ZSkge1xuICAgICAgLy8gSWYgdGhlIHZhbHVlIGRpZG4ndCBjaGFuZ2UgdGhlcmUncyBubyByZWFzb24gdG8gc2VuZCBhbiB1cGRhdGUuXG4gICAgICAvLyBTcGVjaWZpY2FsbHkgdGhpcyBtYXkgaGFwcGVuIGR1cmluZyBhbiB1cGRhdGUgZnJvbSB0aGUgbW9kZWwgKHdyaXRlVmFsdWUpIHdoZXJlIHNlbmRpbmcgYW4gdXBkYXRlIHRvIHRoZSBtb2RlbCB3b3VsZCBhY3R1YWxseSBiZSBpbmNvcnJlY3QuXG4gICAgICByZXR1cm47XG4gICAgfVxuXG4gICAgdGhpcy52YWx1ZSA9IHZhbHVlO1xuICAgIHRoaXMub25DaGFuZ2UodmFsdWUpO1xuICB9XG5cbiAgcHJpdmF0ZSB1cGRhdGVPcHRpb25Ub01vbmFjbygpOiB2b2lkIHtcbiAgICBpZiAodGhpcy5lZGl0b3JJbnN0YW5jZSkge1xuICAgICAgdGhpcy5lZGl0b3JJbnN0YW5jZS51cGRhdGVPcHRpb25zKHsgLi4udGhpcy5lZGl0b3JPcHRpb25DYWNoZWQgfSk7XG4gICAgfVxuICB9XG59XG4iXX0=