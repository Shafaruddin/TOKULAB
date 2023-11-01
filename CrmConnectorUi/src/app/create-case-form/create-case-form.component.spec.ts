import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CreateCaseFormComponent } from './create-case-form.component';

describe('CreateCaseFormComponent', () => {
  let component: CreateCaseFormComponent;
  let fixture: ComponentFixture<CreateCaseFormComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ CreateCaseFormComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(CreateCaseFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
